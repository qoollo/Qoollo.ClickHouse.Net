using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Qoollo.ClickHouse.Net;
using Qoollo.ClickHouse.Net.Repository;
using Serilog;
using WebApiExample.Model;

namespace WebApiExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Action with will be used by workers to process packages from queue
        /// </summary>
        private readonly Action<IClickHouseRepository, List<TestEntity>, Microsoft.Extensions.Logging.ILogger> writeAction = (repository, package, logger) =>
        { 
            repository.BulkInsert(TestEntity.TableName, TestEntity.ColumnNames, package);
            logger.LogInformation($"Package inserted by worker-thread, package.Count: {package.Count}");
        };

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(s => s.AddSerilog());

            // Add QueueProcessor
            services.AddClickHouseRepository(Configuration.GetSection("ClickHouseConnectionPoolConfiguration"));
            services.AddClickHouseAggregatingQueueProcessor(Configuration.GetSection("ClickHouseAggregatingQueueProcessorConfiguration"), writeAction);

            services.AddControllers();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
