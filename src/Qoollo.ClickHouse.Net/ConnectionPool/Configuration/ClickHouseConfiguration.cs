using System.Collections.Generic;

namespace Qoollo.ClickHouse.Net.ConnectionPool.Configuration
{
    /// <summary>
    /// Configuration for ClickHouseConnectionPool
    /// </summary>
    public class ClickHouseConfiguration : IClickHouseConfiguration
    {
        /// <summary>
        /// List of connection strings.
        /// </summary>
        public List<string> ConnectionStrings { get; set; }

        /// <summary>
        /// Maximum elements count for connection pool.
        /// </summary>
        public int ConnectionPoolMaxCount { get; set; }

        /// <summary>
        /// Name for connection pool
        /// </summary>
        public string ConnectionPoolName { get; set; }
    }
}
