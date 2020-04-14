namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor
{
    public enum State
    {
        /// <summary>
        /// The instance is created, but not started
        /// </summary>
        Created,
        /// <summary>
        /// The instance is working and ready to ready to receive items
        /// </summary>
        Started,
        /// <summary>
        /// The instance is stopping, adding items is unavailable
        /// </summary>
        Stopping,
        /// <summary>
        /// The instance is stopped
        /// </summary>
        Stoped,
        /// <summary>
        /// The instance is disposed 
        /// </summary>
        Disposed
    }
}
