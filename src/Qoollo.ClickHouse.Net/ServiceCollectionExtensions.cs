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
    /// <summary>
    /// Extensions to add Qoollo.ClickHouse.Net classes to DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Extension to add ClickHouseConnectionPool and IClickHouseRepository to services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration section that contain connection pool configuration (ClickHouseConnectionPoolConfiguration)</param>
        public static IServiceCollection AddClickHouseRepository(this IServiceCollection services, IConfigurationSection configuration)
        {
            var config = configuration.Get<ClickHouseConnectionPoolConfiguration>();
            services.AddTransient<IClickHouseConnectionPoolConfiguration>(serviceProvider => config);
            services.AddSingleton<ClickHouseConnectionPool>();
            services.AddTransient<IClickHouseRepository, ClickHouseRepository>();
            return services;
        }

        /// <summary>
        /// Extension to add IClickHouseAggregatingQueueProcessor to services. Requires IClickHouseRepository in DI.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration section that contain queue processor configuration (ClickHouseAggregatingQueueProcessorConfiguration)</param>
        /// <param name="proc"></param>
        /// <returns></returns>
        public static IServiceCollection AddClickHouseAggregatingQueueProcessor<T>(
            this IServiceCollection services, 
            IConfigurationSection configuration, 
            Action<IClickHouseRepository, List<T>, ILogger> proc)
        {
            var processorConfig = configuration.Get<ClickHouseAggregatingQueueProcessorConfiguration>();
            services.AddTransient<IClickHouseAggregatingQueueProcessorConfiguration>(serviceProvider => processorConfig);
            services.AddTransient<IProcHolder<T>>(serviceProvider => new ProcHolder<T>(proc));
            services.AddSingleton<IClickHouseAggregatingQueueProcessor<T>, ClickHouseAggregatingQueueProcessor<T>>();
            return services;
        }
    }
}
