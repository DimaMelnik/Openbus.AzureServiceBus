using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Openbus.AzureServiceBus.Extensions;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Pipeline;
using Openbus.AzureServiceBus.Validator;
using Openbus.Example.Handlers;
using Openbus.Example.Messages;
using Openbus.Example.Transport;
using Openbus.Example.Validators;

namespace Openbus.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build configuration to get serilog settings (https://github.com/serilog/serilog-settings-configuration)
            var configuration = CreateConfigurationBuilder().Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IConfigurationBuilder CreateConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json",
                    true, true)
                .AddEnvironmentVariables();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(ConfigureServices)
                .ConfigureServices(ConfigureCountdownServiceBus);
        }


        public static void ConfigureCountdownServiceBus(HostBuilderContext context, IServiceCollection services)
        {
            services.AddQueueBus<IMyQueue>(context.Configuration.GetSection("ServiceBusQueue"))
                .WithMessage<OrderPlacedEvent>(
                    c =>
                    {
                    },
                    c =>
                    {
                        c.SetExponentialRetryOn<KeyNotFoundException>(1, 1000, 2);
                    })
                .AddMessageProcessor();

            services.AddTopicBus<IMyTopic>(context.Configuration.GetSection("ServiceBusOrder"))
                .WithMessage<OrderPlacedEvent>(
                    c =>
                    {
                        c.CompleteOnConvertionError = true;
                        c.CompleteOnValidationError = true;
                    },
                    c =>
                    {
                        c.SetImediateRetryOn<Exception>(2);
                    })
                .AddMessageProcessor();
            
            services.AddTopicBus<IMySessionTopic>(context.Configuration.GetSection("ServiceBusOrderSessions"))
                .WithMessage<OrderPlacedEvent, OrderState>(
                    c =>
                    {
                        c.CompleteOnConvertionError = true;
                        c.CompleteOnValidationError = true;
                    },
                    c =>
                    { 
                        c.SetImediateRetryOn<Exception>(2);
                    })
                .AddSessionMessageProcessor();
        }

        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            //Converters
            services
                .AddTransient<IMessageConverter<IMyQueue, OrderPlacedEvent>, OrderPlacedEventConverter<IMyQueue>>();
            services
                .AddTransient<IMessageConverter<IMySessionTopic, OrderPlacedEvent>, OrderPlacedEventConverter<IMySessionTopic>>();
            services
                .AddTransient<IMessageConverter<IMyTopic, OrderPlacedEvent>, OrderPlacedEventConverter<IMyTopic>>();

            //Handlers
            services
                .AddTransient<IMessageHandler<IMyQueue, OrderPlacedEvent>, OrderPlacedQueueHandler>();
            services
                .AddTransient<IMessageHandler<IMyTopic, OrderPlacedEvent>, OrderPlacedTopicHandler>();
            services
                .AddTransient<IMessageHandler<IMySessionTopic, OrderPlacedEvent, OrderState>,
                    OrderPlacedSessionTopicHandler>();
            
            //Validators
            services.AddTransient<IMessageValidator<OrderPlacedEvent>, TestEventValidator>();

            services.AddHostedService<Worker>();
        }
    }
}