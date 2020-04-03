using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qoollo.ClickHouse.Net;
using Qoollo.ClickHouse.Net.Repository;
using WebApiExample.Model;

namespace WebApiExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                .Build();

            // init database
            CreateClickHouseDatabase(host);

            //use extention to start processor
            host.StartClickHouseAggregatingQueueProcessor<TestEntity>().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Method to create database if it is not exists
        /// </summary>
        /// <param name="host"></param>
        public static void CreateClickHouseDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var repository = services.GetService<IClickHouseRepository>();
            repository.ExecuteNonQuery(TestEntity.CreateTableQuery);
        }
    }
}
