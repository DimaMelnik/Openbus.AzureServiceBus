using Azure.Messaging.ServiceBus;
using System;
using Openbus.AzureServiceBus.Extensions;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.Example.Messages
{
    public class OrderPlacedEventConverter<TBus> :
        MessageConverter<TBus, OrderPlacedEvent>
        where TBus : IBus
    {
        private const string EventType = "OrderPlaced";
        public override ServiceBusMessage Serialize(OrderPlacedEvent message)
        {
            var sbMessage = base.Serialize(message);

            sbMessage.ApplicationProperties.Add(UserPropertyKeys.SOURCE, "Example");
            sbMessage.ApplicationProperties.Add(UserPropertyKeys.EVENT_TYPE, EventType);
            sbMessage.MessageId = Guid.NewGuid().ToString();
            sbMessage.SessionId = $"{EventType}/{message.Id}";

            return sbMessage;
        }

        public override bool CanDeserialize(ServiceBusReceivedMessage message) =>
            message.ApplicationProperties.ContainsKeyValuePair(UserPropertyKeys.SOURCE, "Example") &&
            message.ApplicationProperties.ContainsKeyValuePair(UserPropertyKeys.EVENT_TYPE, EventType);
    }
}