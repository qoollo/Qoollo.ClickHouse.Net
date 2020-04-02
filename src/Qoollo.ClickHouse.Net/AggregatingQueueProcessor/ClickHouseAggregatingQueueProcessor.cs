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
    /// Items are aggregated to packages and pushed to the processing queue. Worker-threads process packages using given Action.
    /// A package is sent to the processing queue either when a sufficient number of elements are dialed, or by timer.
    /// </summary>
    /// <typeparam name="T">Type of items for aggregation</typeparam>
    public class ClickHouseAggregatingQueueProcessor<T> : IClickHouseAggregatingQueueProcessor<T>
    {
        private readonly DelegateQueueAsyncProcessor<List<T>> _delegateQueueAsyncProcessor;
        private readonly Action<IClickHouseRepository, List<T>, ILogger> _proc;
        private readonly ConcurrentQueue<T> _elementQueue = new ConcurrentQueue<T>();

        private readonly IClickHouseRepository _repository;
        private readonly ILogger _logger;
        private readonly object _lock = new object();
        
        private volatile int _totalProcessedEventsCount;
        private volatile int _totalPushedEventsCount;
        private System.Timers.Timer _timer;

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
        public int TotalPushedItemsCount => _totalPushedEventsCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"> Configuration for instance </param>
        /// <param name="repository"> IClickHouseRepository instance </param>
        /// <param name="proc"> The action that the Worker-threads will perform with packages from the processing queue </param>
        /// <param name="logger">Logger</param>
        public ClickHouseAggregatingQueueProcessor(
            IClickHouseAggregatingQueueProcessorConfiguration config,
            IClickHouseRepository repository, 
            Action<IClickHouseRepository, List<T>, ILogger> proc, 
            ILogger logger)
        {
            _logger = logger;
            MaxPackageSize = config.MaxPackageSize;
            _repository = repository;
            _proc = proc;
            TimerPeriodMs = config.TimerPeriodMs;

            _delegateQueueAsyncProcessor = new DelegateQueueAsyncProcessor<List<T>>(
                threadCount: config.ProcessingThreadsCount,
                maxQueueSize: config.QueueMaxSize,
                name: "AggregatingQueueProcessor",
                processing: ThreadProc);

            InitTimer(TimerPeriodMs);

            State = State.Created;
        }

        /// <summary>
        /// Add an item to the next package that is being prepared to be added to the processing queue
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        public void Add(T item)
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");

            _elementQueue.Enqueue(item);

            if (_elementQueue.Count >= MaxPackageSize)
            {
                lock (_lock)
                {
                    if (_elementQueue.Count >= MaxPackageSize)
                        AddSinglePackage();
                }
            }
        }

        /// <summary>
        /// Add the package to the processing queue
        /// </summary>
        /// <param name="package"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Package size is too big </exception>
        public void AddPackage(List<T> package)
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");

            if (package.Count > MaxPackageSize)
                throw new ArgumentOutOfRangeException(nameof(package), "package size is upper then queue MaxPackageSize");

            _delegateQueueAsyncProcessor.Add(package);
        }

        /// <summary>
        /// Force complete current package and add it to the processing queue
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        public void PushPackageToQueue()
        {
            if (State != State.Started)
                throw new InvalidOperationException("AggregatingQueueProcessor is not statred");
            
            AddSinglePackage();
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
            _totalPushedEventsCount = 0;
            _delegateQueueAsyncProcessor.Start();
            _timer.Enabled = true;
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
            lock (_lock)
            {
                while (_elementQueue.Count > 0)
                    AddSinglePackage();
            }
            _timer.Enabled = false;
            _delegateQueueAsyncProcessor.Stop(waitForStop: true, letFinishProcess: true, completeAdding: true);
            State = State.Stoped;
        }

        private void AddSinglePackage()
        {
            var package = new List<T>(MaxPackageSize);
            while (_elementQueue.TryDequeue(out var elem) && package.Count < MaxPackageSize)
                package.Add(elem);

            _delegateQueueAsyncProcessor.Add(package);
            Interlocked.Add(ref _totalPushedEventsCount, package.Count);
        }

        private void ThreadProc(List<T> package, CancellationToken token)
        {
            try
            {
                if (package.Count > 0)
                {
                    _proc(_repository, package, _logger);
                    Interlocked.Add(ref _totalProcessedEventsCount, package.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured");
                throw;
            }
        }

        #region Timer
        private void InitTimer(int timerPeriodMs)
        {
            if (State != State.Created)
                throw new InvalidOperationException("AggregatingQueueProcessor is already started");

            _timer = new System.Timers.Timer(timerPeriodMs);

            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            AddSinglePackage();
        }
        #endregion

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (State == State.Disposed)
                return;

            if (disposing)
            {
                if (State == State.Started)
                    Stop();

                _timer?.Dispose();
                _delegateQueueAsyncProcessor?.Dispose();
            }

            State = State.Disposed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
