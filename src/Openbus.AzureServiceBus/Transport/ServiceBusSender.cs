using Azure.Messaging.ServiceBus;

namespace Openbus.AzureServiceBus.Transport
{
    public class ServiceBusSender<T> where T : IBus
    {
        public ServiceBusSender(ServiceBusSender sender)
        {
            Sender = sender;
        }

        public ServiceBusSender Sender { get; set; }
    }
}