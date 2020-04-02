namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor
{
    /// <summary>
    /// Configuration for ClickHouseAggregatingQueueProcessor
    /// </summary>
    public class ClickHouseAggregatingQueueProcessorConfiguration : IClickHouseAggregatingQueueProcessorConfiguration
    {
        /// <summary>
        /// Max size of the package that can be added to the queue 
        /// </summary>
        public int MaxPackageSize { get; set; }

        /// <summary>
        /// Count of worker-threads
        /// </summary>
        public int ProcessingThreadsCount { get; set; }

        /// <summary>
        /// Max size of processing-queue
        /// </summary>
        public int QueueMaxSize { get; set; }

        /// <summary>
        /// Time in ms for package sending timer
        /// </summary>
        public int TimerPeriodMs { get; set; }
    }
}