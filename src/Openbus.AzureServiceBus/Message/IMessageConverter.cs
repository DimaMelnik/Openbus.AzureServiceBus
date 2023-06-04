using Azure.Messaging.ServiceBus;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Message
{
    public interface IMessageConverter<TBus, TMessage>
        where TMessage : IMessage
        where TBus : IBus
    {
        public TMessage Deserialize(ServiceBusReceivedMessage receivedMessage);
        public ServiceBusMessage Serialize(TMessage message);
        public bool CanDeserialize(ServiceBusReceivedMessage message);
    }
}