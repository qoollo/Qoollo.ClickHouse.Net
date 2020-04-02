using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.AggregatingQueueProcessor;
using Qoollo.ClickHouse.Net.AggregatingQueueProcessor.Configuration;
using Qoollo.ClickHouse.Net.ConnectionPool;
using Qoollo.ClickHouse.Net.ConnectionPool.Configuration;
using Qoollo.ClickHouse.Net.Repository;
using System;
using System.Collections.Generic;

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

        public static void AddClickHouseRepositoryAndQueue<T>(
            this IServiceCollection services, 
            IConfigurationSection configuration, 
            Action<IClickHouseRepository, List<T>, ILogger> proc)
        {
            services.AddClickHouseRepository(configuration.GetSection(nameof(ClickHouseConfiguration)));

            var config = configuration.Get<ClickHouseAggregatingQueueProcessorConfiguration>();
            services.AddTransient<IClickHouseAggregatingQueueProcessorConfiguration>(serviceProvider => config);

            services.AddSingleton<IClickHouseAggregatingQueueProcessor<T>>(serviceProvider => new ClickHouseAggregatingQueueProcessor<T>(
                config: config,
                repository: serviceProvider.GetRequiredService<IClickHouseRepository>(),
                proc: proc,
                logger: serviceProvider.GetRequiredService<ILogger>()));
        }
    }
}
