using System;
using System.Collections.Generic;

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
    public interface IClickHouseAggregatingQueueProcessor<T> : IDisposable
    {
        /// <summary>
        /// Add an item to the preparation queue. that is being prepared to be added to the processing queue
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        void Add(T item);

        /// <summary>
        /// Add collection of items to the preparation queue
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        void AddCollection(ICollection<T> collection);

        /// <summary>
        /// Push the package to the processing queue. WARNING - this method push package direct to the processing queue. 
        /// So all items from preparation queue(after timer event) will be after that package in the processing queue, not before!
        /// </summary>
        /// <param name="package"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Package size is too big </exception>
        void PushPackage(List<T> package);

        /// <summary>
        /// Creates packages using all items from preparation queue and pushes them to the processing queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        void ForcePushAllPreparedPackages();

        /// <summary>
        /// Start processing
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        void Start();

        /// <summary>
        /// Stop processing 
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action  </exception>
        void Stop();

        /// <summary>
        /// Current state
        /// </summary>
        public State State { get; }
        
        /// <summary>
        /// Count of packages in the processing queue
        /// </summary>
        int CurrentQueueSize { get; }

        /// <summary>
        /// Max size of the package that can be added to the queue 
        /// </summary>
        int MaxPackageSize { get; }

        /// <summary>
        /// Total count of items, that was processed
        /// </summary>
        int TotalProcessedItemsCount { get; }

        /// <summary>
        /// Total count of items, that was added to the processing queue
        /// </summary>
        int TotalProcessingQueuePushCount { get; }

        /// <summary>
        /// Total count of items, that was added by any Add or Push method
        /// </summary>
        int TotalAddedItemsCount { get; }

        /// <summary>
        /// Time in ms for package sending timer
        /// </summary>
        public int TimerPeriodMs { get; }
    }
}
