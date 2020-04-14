using System;
using System.Runtime.Serialization;

namespace Qoollo.ClickHouse.Net.ConnectionPool
{
    /// <summary>
    /// ClickHouse Connection Error Exception
    /// </summary>
    [Serializable]
    public class ClickHouseConnectionException : Exception
    {
        /// <summary>
        /// ClickHouse Connection Error Exception
        /// </summary>
        public ClickHouseConnectionException()
        { }

        /// <summary>
        /// ClickHouse Connection Error Exception
        /// </summary>
        /// <param name="message"></param>
        public ClickHouseConnectionException(string message) : base(message)
        { }

        /// <summary>
        /// ClickHouse Connection Error Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ClickHouseConnectionException(string message, Exception innerException) : base(message, innerException)
        { }

        /// <summary>
        /// ClickHouse Connection Error Exception
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ClickHouseConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
