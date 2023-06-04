using System;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Configuration
{
    public class ServiceBusTopicConfiguration<T> : ServiceBusConfigurationBase<T>
        where T : IBus
    {
        public string Topic { get; set; }
        public string Subscription { get; set; }
    }
}