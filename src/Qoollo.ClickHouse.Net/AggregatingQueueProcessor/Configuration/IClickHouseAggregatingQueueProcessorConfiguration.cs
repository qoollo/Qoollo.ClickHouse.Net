namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor.Configuration
{
    /// <summary>
    /// Configuration for ClickHouseAggregatingQueueProcessor
    /// </summary>
    public interface IClickHouseAggregatingQueueProcessorConfiguration
    {
        /// <summary>
        /// Max size of the package that can be added to the queue 
        /// </summary>
        int MaxPackageSize { get; set; }

        /// <summary>
        /// Count of worker-threads
        /// </summary>
        int ProcessingThreadsCount { get; set; }

        /// <summary>
        /// Max size of processing-queue
        /// </summary>
        int QueueMaxSize { get; set; }

        /// <summary>
        /// Time in ms for package sending timer
        /// </summary>
        int TimerPeriodMs { get; set; }
    }
}