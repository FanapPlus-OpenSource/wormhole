﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using CommonLogic.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Wormhole.DataImplementation;
using Wormhole.Job;
using Wormhole.Kafka;
using Wormhole.Utils;

namespace Wormhole.Worker
{
    internal class Program
    {
        private static readonly NebulaContext NebulaContext = new NebulaContext();
        private static readonly IConfigurationRoot AppConfiguration = BuildConfiguration(Directory.GetCurrentDirectory());
        private static readonly ServiceProvider ServiceProvider = ConfigureServices();
        private static ILogger<Program> Logger { get; set; }

        public static void Main(string[] args)
        {
            ConfigureLogging();

            ConfigureNebula();
            AppSettingsProvider.MongoConnectionString =
                AppConfiguration.GetConnectionString(Constants.MongoConnectionString);
            StartNebulaService();
            var topics = GetTopics().GetAwaiter().GetResult();
            StartConsuming(topics);

        }

        private static async Task<List<string>> GetTopics()
        {
            var tenantDa = ServiceProvider.GetService<ITenantDA>();
            var topics = await tenantDa.FindTenants();
            return topics.Select(t=>t.Name).ToList();
        }


        private static void StartConsuming(List<string> topics)
        {
            var consumer = ServiceProvider.GetService<IKafkaConsumer<Null, string>>();
            ICollection<KeyValuePair<string, object>> config = new Collection<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("group.id","GroupId")
            };
            consumer.Initialize(config, OnMessageEventHandler);
            try
            {
                consumer.Subscribe(topics);
                while (true) consumer.Poll(TimeSpan.FromMilliseconds(500));
            }
            finally
            {
                StopNebulaService();
                consumer.Dispose();
            }
        }


        private static void StartNebulaService()
        {
            if (!Environment.UserInteractive)
            {
                // running as service
                Logger.LogInformation("Windows service starting");
                using (var windowsService = new JobQueueWindowsService(NebulaContext))
                {
                    ServiceBase.Run(windowsService);
                }

                Logger.LogInformation("Windows service started");
            }
            else
            {
                // running as console app

                Console.WriteLine("Wormhole.Worker worker service...");
                NebulaContext.StartWorkerService();
                Console.WriteLine("Service started. Press ENTER to stop.");
            }
        }

        private static void ConfigureLogging()
        {
            ServiceProvider
                .GetService<ILoggerFactory>()
                .AddLog4Net();

            Logger = ServiceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            Logger.LogDebug("Starting application");
        }


        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<ITenantDA, TenantDA>()
                .AddSingleton<IKafkaConsumer<Null, string>, KafkaConsumer>()
                .Configure<KafkaConfig>(AppConfiguration.GetSection(Constants.KafkaConfig))
                .AddSingleton<IFinalizableJobProcessor<OutgoingQueueStep>, OutgoingQueueProcessor>()
                .BuildServiceProvider();
        }

        private static void ConfigureNebula()
        {
            NebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            NebulaContext.MongoConnectionString =
                AppConfiguration.GetConnectionString("nebula:mongoConnectionString");
            NebulaContext.RedisConnectionString =
                AppConfiguration.GetConnectionString("nebula:redisConnectionString");
            
            var delayedQueueProcessor = ServiceProvider.GetService<IFinalizableJobProcessor<OutgoingQueueStep>>();
            NebulaContext.RegisterJobProcessor(delayedQueueProcessor, typeof(OutgoingQueueStep));
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", true, true);

            if (!string.IsNullOrWhiteSpace(environmentName)) builder = builder.AddJsonFile($"appsettings.{environmentName}.json", true);

            builder = builder.AddEnvironmentVariables();
            
            return builder.Build();
        }

        private static void OnMessageEventHandler(object sender, Message<Null, string> message)
        {
            Logger.LogDebug(message.Value);
        }


        private static void StopNebulaService()
        {
            Console.WriteLine("Stopping the serivce...");
            NebulaContext.StopWorkerService();
            Console.WriteLine("Service stopped, everything looks clean.");
        }
    }
}
