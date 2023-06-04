using Azure.Messaging.ServiceBus;

namespace Openbus.AzureServiceBus.Transport
{
    public class ServiceBusClient<T> : ServiceBusClient where T : IBus
    {
        public ServiceBusClient(string connectionString) : base(connectionString)
        {
        }
        public ServiceBusClient(string connectionString, ServiceBusClientOptions options) : base(connectionString, options)
        {
        }
    }
}