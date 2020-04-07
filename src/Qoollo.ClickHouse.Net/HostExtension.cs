using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qoollo.ClickHouse.Net.AggregatingQueueProcessor;

namespace Qoollo.ClickHouse.Net
{
    /// <summary>
    /// Extension to start ClickHouseAggregatingQueueProcessor
    /// </summary>
    public static class HostExtension
    {
        /// <summary>
        /// Start ClickHouseAggregatingQueueProcessor added in ConfigureServices.
        /// </summary>
        /// <typeparam name="T">ClickHouseAggregatingQueueProcessor entity type</typeparam>
        /// <param name="host"></param>
        public static IHost StartClickHouseAggregatingQueueProcessor<T>(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var databaseMaintenanceService = services.GetService<IClickHouseAggregatingQueueProcessor<T>>();
                databaseMaintenanceService.Start();
            }
            return host;
        }
    }
}
