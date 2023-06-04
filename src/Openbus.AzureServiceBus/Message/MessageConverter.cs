using System;
using System.Net.Mime;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Message
{
    public abstract class MessageConverter<TBus, TMessage> : IMessageConverter<TBus, TMessage>
        where TMessage : IMessage
        where TBus : IBus
    {
        public virtual TMessage Deserialize(ServiceBusReceivedMessage receivedMessage)
        {
            return JsonConvert.DeserializeObject<TMessage>(receivedMessage.Body.ToString());
        }

        public virtual ServiceBusMessage Serialize(TMessage message)
        {
            var correlationId = Guid.NewGuid().ToString("N");
            var messageToSend = JsonConvert.SerializeObject(message);
            var sbMessage = new ServiceBusMessage(messageToSend)
            {
                MessageId = Guid.NewGuid().ToString("N"),
                ContentType = $"{MediaTypeNames.Application.Json}",
                CorrelationId = correlationId
            };

            return sbMessage;
        }

        public abstract bool CanDeserialize(ServiceBusReceivedMessage message);
    }
}