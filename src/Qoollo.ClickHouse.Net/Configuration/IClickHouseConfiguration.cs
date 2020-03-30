using System.Collections.Generic;

namespace Qoollo.ClickHouse.Net.Configuration
{
    /// <summary>
    /// Configuration for ClickHouseConnectionPool
    /// </summary>
    public interface IClickHouseConfiguration
    {
        /// <summary>
        /// List of connection strings.
        /// </summary>
        List<string> ConnectionStrings { get; set; }

        /// <summary>
        /// Maximum elements count for connection pool.
        /// </summary>
        int ConnectionPoolMaxCount { get; set; }
    }
}
