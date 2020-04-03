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
        public static IServiceCollection AddClickHouseRepository(this IServiceCollection services, IConfigurationSection configuration)
        {
            var config = configuration.Get<ClickHouseConfiguration>();
            services.AddTransient<IClickHouseConfiguration>(serviceProvider => config);
            services.AddSingleton<ClickHouseConnectionPool>();
            services.AddTransient<IClickHouseRepository, ClickHouseRepository>();
            return services;
        }

        public static IServiceCollection AddClickHouseRepositoryAndQueueProcessor<T>(
            this IServiceCollection services, 
            IConfigurationSection configuration, 
            Action<IClickHouseRepository, List<T>, ILogger> proc)
        {
            var poolSection = configuration.GetSection(nameof(ClickHouseConfiguration));
            var processorSection = configuration.GetSection(nameof(ClickHouseAggregatingQueueProcessorConfiguration));
            services.AddClickHouseRepository(poolSection);
            var processorConfig = processorSection.Get<ClickHouseAggregatingQueueProcessorConfiguration>();
            services.AddTransient<IClickHouseAggregatingQueueProcessorConfiguration>(serviceProvider => processorConfig);
            services.AddTransient<IProcHolder<T>>(serviceProvider => new ProcHolder<T>(proc));
            services.AddSingleton<IClickHouseAggregatingQueueProcessor<T>, ClickHouseAggregatingQueueProcessor<T>>();
            return services;
        }
    }
}
