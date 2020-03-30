using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.Configuration;
using Qoollo.ClickHouse.Net.ConnectionPool;
using Qoollo.ClickHouse.Net.Repository;

namespace Qoollo.ClickHouse.Net
{
    public static class ServiceCollectionExtensions
    {
        public static void AddClickHouseRepository(this IServiceCollection services, IConfigurationSection configuration)
        {
            var config = configuration.Get<ClickHouseConfiguration>();
            services.AddTransient<IClickHouseConfiguration>(serviceProvider => config);

            services.AddSingleton(serviceProvider => new ClickHouseConnectionPool(
                config: config,
                name: "ClickHouseConnectionPool",
                logger: serviceProvider.GetRequiredService<ILogger>()));

            services.AddTransient<IClickHouseRepository, ClickHouseRepository>();
        }
    }
}
