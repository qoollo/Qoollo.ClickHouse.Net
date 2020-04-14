using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.AggregatingQueueProcessor.Configuration;
using Qoollo.ClickHouse.Net.Repository;
using Qoollo.Turbo.Threading.QueueProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor
{
    /// <summary>
    /// Aggregating queue with a set of worker threads. 
    ///
    /// Contain two queues:
    /// <para> 1) processing queue - contain packages for processing by a set of worker-threads. </para>
    /// <para> 2) preparation queue - to create the next packages for processing queue. </para> 
    /// Added items are pushed to the preparation queue.By timer event (or using ForcePushAllPreparedPackages). 
    /// Items from preparation queue are aggregated to packages and pushed to the processing queue.
    /// Worker-threads process packages from processing queue using given Action.
    /// </summary>
    /// <typeparam name="T">Type of items for aggregation</typeparam>
    public class ClickHouseAggregatingQueueProcessor<T> : IClickHouseAggregatingQueueProcessor<T>
    {
        private readonly DelegateQueueAsyncProcessor<List<T>> _delegateQueueAsyncProcessor;
        private readonly System.Timers.Timer _addPackageTimer;
        private readonly ConcurrentQueue<T> _elementQueue = new ConcurrentQueue<T>();

        private readonly IProcHolder<T> _procHolder;
        private readonly IClickHouseRepository _repository;
        private readonly ILogger<ClickHouseAggregatingQueueProcessor<T>> _logger;
        
        private volatile int _totalProcessedEventsCount;
        private volatile int _totalProcessingQueuePushCount;
        private volatile int _totalAddedItemsCount;

        /// <summary>
        /// Max size of the package that can be added to the queue 
        /// </summary>
        public int MaxPackageSize { get; }
        /// <summary>
        /// Time in ms for package sending timer
        /// </summary>
        public int TimerPeriodMs { get; }
        /// <summary>
        /// Current state
        /// </summary>
        public State State { get; private set; }
        /// <summary>
        /// Count of packages in the processing queue
        /// </summary>
        public int CurrentQueueSize => _delegateQueueAsyncProcessor.ElementCount;
        /// <summary>
        /// Total count of items, that was processed
        /// </summary>
        public int TotalProcessedItemsCount => _totalProcessedEventsCount;
        /// <summary>
        /// Total count of items, that was added to the processing queue
        /// </summary>
        public int TotalProcessingQueuePushCount => _totalProcessingQueuePushCount;
        /// <summary>
        /// Total count of items, that was added by any Add or Push method
        /// </summary>
        public int TotalAddedItemsCount => _totalAddedItemsCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"> Configuration for instance </param>
        /// <param name="repository"> IClickHouseRepository instance </param>
        /// <param name="procHolder"> Holder for action that the worker-threads will perform with packages from the processing queue </param>
        /// <param name="logger">Logger</param>
        public ClickHouseAggregatingQueueProcessor(
            IClickHouseAggregatingQueueProcessorConfiguration config,
            IClickHouseRepository repository, 
            IProcHolder<T> procHolder,
            ILogger<ClickHouseAggregatingQueueProcessor<T>> logger)
        {
            _logger = logger;
            MaxPackageSize = config.MaxPackageSize;
            _repository = repository;
            _procHolder = procHolder;
            TimerPeriodMs = config.TimerPeriodMs;

            _delegateQueueAsyncProcessor = new DelegateQueueAsyncProcessor<List<T>>(
                threadCount: config.ProcessingThreadsCount,
                maxQueueSize: config.QueueMaxSize,
                name: "AggregatingQueueProcessor",
                processing: ThreadProc);

            _addPackageTimer = new System.Timers.Timer(TimerPeriodMs);

            _addPackageTimer.Elapsed += OnTimedEvent;
            _addPackageTimer.AutoReset = true;
            _addPackageTimer.Enabled = false;

            State = State.Created;
        }

        /// <summary>
        /// Add an item to the preparation queue. that is being prepared to be added to the processing queue
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        public void Add(T item)
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");

            _elementQueue.Enqueue(item);
            Interlocked.Add(ref _totalAddedItemsCount, 1);
        }

        /// <summary>
        /// Add collection of items to the preparation queue
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        public void AddCollection(ICollection<T> collection)
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");

            foreach (var item in collection)
                _elementQueue.Enqueue(item);
            Interlocked.Add(ref _totalAddedItemsCount, collection.Count);
        }

        /// <summary>
        /// Push the package to the processing queue. WARNING - this method push package direct to the processing queue. 
        /// So all items from preparation queue(after timer event) will be after that package in the processing queue, not before!
        /// </summary>
        /// <param name="package"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Package size is too big </exception>
        public void PushPackage(List<T> package)
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");

            if (package.Count > MaxPackageSize)
                throw new ArgumentOutOfRangeException(nameof(package), "package size is upper then queue MaxPackageSize");

            _delegateQueueAsyncProcessor.Add(package);
            _logger.LogInformation("Package with {0} elements was added to processong queue", package.Count);
            Interlocked.Add(ref _totalAddedItemsCount, package.Count);
            Interlocked.Add(ref _totalProcessingQueuePushCount, package.Count);
        }

        /// <summary>
        /// Creates packages using all items from preparation queue and pushes them to the processing queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        public void ForcePushAllPreparedPackages()
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");

            PushAllPreparedPackages();
        }

        /// <summary>
        /// Start processing
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action  </exception>
        public void Start()
        {
            if (State != State.Created)
                throw new InvalidOperationException("AggregatingQueueProcessor is already strated");

            _totalProcessedEventsCount = 0;
            _totalProcessingQueuePushCount = 0;
            _delegateQueueAsyncProcessor.Start();
            _addPackageTimer.Enabled = true;
            State = State.Started;
        }

        /// <summary>
        /// Stop processing 
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action  </exception>
        public void Stop()
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not strated");

            State = State.Stopping;
            _addPackageTimer.Enabled = false;
            
            while (_elementQueue.Count > 0)
                AddSinglePackage();

            _delegateQueueAsyncProcessor.Stop(waitForStop: true, letFinishProcess: true, completeAdding: true);
            State = State.Stoped;
        }

        private void PushAllPreparedPackages()
        {
            var packagesCount = Math.Ceiling(_elementQueue.Count / (float) MaxPackageSize);
            for (int i = 0; i < packagesCount; i++)
                AddSinglePackage();
        }

        private void AddSinglePackage()
        {
            var package = new List<T>(MaxPackageSize);
            while (_elementQueue.TryDequeue(out var elem) && package.Count < MaxPackageSize)
                package.Add(elem);

            _delegateQueueAsyncProcessor.Add(package);
            _logger.LogInformation("Package with {0} elements was added to processong queue", package.Count);
            Interlocked.Add(ref _totalProcessingQueuePushCount, package.Count);
        }

        private void ThreadProc(List<T> package, CancellationToken token)
        {
            try
            {
                if (package.Count > 0)
                {
                    _procHolder.Proc(_repository, package, _logger);
                    Interlocked.Add(ref _totalProcessedEventsCount, package.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured");
                throw;
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            PushAllPreparedPackages();
        }

        #region IDisposable Support
        /// <summary>
        /// Dispose pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (State == State.Disposed)
                return;

            if (disposing)
            {
                if (State == State.Started)
                    Stop();

                _addPackageTimer?.Dispose();
                _delegateQueueAsyncProcessor?.Dispose();
            }

            State = State.Disposed;
        }

        /// <summary>
        /// Dispose processor
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
