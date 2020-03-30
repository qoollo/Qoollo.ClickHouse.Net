using System.Collections.Generic;

namespace Qoollo.ClickHouse.Net.Configuration
{
    public class ClickHouseConfiguration : IClickHouseConfiguration
    {
        public List<string> ConnectionStrings { get; set; }

        public int ConnectionPoolMaxCount { get; set; }
    }
}
