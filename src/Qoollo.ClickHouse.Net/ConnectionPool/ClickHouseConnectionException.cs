using System;
using System.Runtime.Serialization;

namespace Qoollo.ClickHouse.Net.ConnectionPool
{
    [Serializable]
    public class ClickHouseConnectionException : Exception
    {
        public ClickHouseConnectionException()
        { }

        public ClickHouseConnectionException(string message) : base(message)
        { }

        public ClickHouseConnectionException(string message, Exception innerException) : base(message, innerException)
        { }

        protected ClickHouseConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
