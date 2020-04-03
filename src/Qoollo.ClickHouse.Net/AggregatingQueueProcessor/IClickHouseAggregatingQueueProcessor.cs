using System;
using System.Collections.Generic;

namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor
{
    /// <summary>
    /// Aggregating queue with a set of worker threads. 
    /// Items are aggregated to packages and pushed to the processing queue.Worker-threads process packages.
    /// </summary>
    /// <typeparam name="T">Type of items for aggregation</typeparam>
    public interface IClickHouseAggregatingQueueProcessor<T> : IDisposable
    {
        /// <summary>
        /// Add an item to the next package that is being prepared to be added to the processing queue
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        void Add(T item);

        /// <summary>
        /// Add the package to the processing queue
        /// </summary>
        /// <param name="package"></param>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void AddPackage(List<T> package);

        /// <summary>
        /// Force complete current package and add it to the processing queue
        /// </summary>
        /// <exception cref="InvalidOperationException"> Instance state is not correct for this action </exception>
        void PushPackageToQueue();

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
        int TotalPushedItemsCount { get; }

        /// <summary>
        /// Time in ms for package sending timer
        /// </summary>
        public int TimerPeriodMs { get; }
    }
}
