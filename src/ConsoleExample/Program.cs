using ConsoleExample.Readers;
using ConsoleExample.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qoollo.ClickHouse.Net;
using System;
using Serilog;
using System.Collections.Generic;

namespace ConsoleExample
{
    class Program
    {
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            IConfiguration config = new ConfigurationBuilder()
                  .AddJsonFile("config.json", false, false)
                  .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddSerilog())
                .AddClickHouseRepository(config.GetSection("ClickHouseConnectionPoolConfiguration"))
                .AddTransient<IWriter, Writer>()
                .AddTransient<IReader, Reader>()
                .BuildServiceProvider();

            var writer = serviceProvider.GetService<IWriter>();

            //test data
            var entities = new List<Entity>()
            {
                new Entity(1, DateTime.Now, 1, 45.5, 57.55),
                new Entity(1, DateTime.Now, 1, 45.55, 57.55),
                new Entity(1, DateTime.Now, 2, 45.555, 57.55),
                new Entity(1, DateTime.Now, 2, 45.5555, 57.55),
            };

            var entitiesForAsync = new List<Entity>()
            {
                new Entity(2, DateTime.Now, 3, 33.3, 57.55),
                new Entity(3, DateTime.Now, 1, 34.1, 57.55),
            };

            writer.CreateTableIfNotExists();
            //sync bulk insert
            writer.WriteEntities(entities);

            //async bulk insert
            writer.WriteEntitiesAsync(entitiesForAsync).Wait();

            var reader = serviceProvider.GetService<IReader>();

            var entitiesByUserId = reader.SelectEntitiesByUserId(1);
            
            foreach (var item in entitiesByUserId)
                Console.WriteLine(item);
        }
    }
}
