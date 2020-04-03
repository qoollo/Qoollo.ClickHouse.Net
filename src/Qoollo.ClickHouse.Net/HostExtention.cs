using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qoollo.ClickHouse.Net.AggregatingQueueProcessor;

namespace Qoollo.ClickHouse.Net
{
    public static class HostExtention
    {
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
