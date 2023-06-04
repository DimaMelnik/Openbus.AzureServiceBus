using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Configuration
{
    public class ServiceBusQueueConfiguration<T> : ServiceBusConfigurationBase<T>
        where T : IBus
    {
        public string Queue { get; set; }
    }
}