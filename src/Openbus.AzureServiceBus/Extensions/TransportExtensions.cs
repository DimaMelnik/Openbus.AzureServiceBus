using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Extensions
{
    public static class TransportExtensions
    {
        public static TransportBuilder<TBus> AddTopicBus<TBus>(this IServiceCollection services,
            IConfiguration namedConfigurationSection)
            where TBus : IBusTopic
        {
            services.Configure<ServiceBusTopicConfiguration<TBus>>(namedConfigurationSection);
            var serviceBusConfiguration = services.BuildServiceProvider()
                .GetService<IOptions<ServiceBusTopicConfiguration<TBus>>>().Value;

            var transportBuilder = new TransportBuilder<TBus>(services, serviceBusConfiguration);

            transportBuilder.AddTopicBus();
            return transportBuilder;
        }

        public static TransportBuilder<TBus> AddQueueBus<TBus>(this IServiceCollection services,
            IConfiguration namedConfigurationSection)
            where TBus : IBusQueue
        {
            services.Configure<ServiceBusQueueConfiguration<TBus>>(namedConfigurationSection);
            var serviceBusConfiguration = services.BuildServiceProvider()
                .GetService<IOptions<ServiceBusQueueConfiguration<TBus>>>().Value;

            var transportBuilder = new TransportBuilder<TBus>(services, serviceBusConfiguration);

            transportBuilder.AddQueueBus();
            return transportBuilder;
        }
    }
}