using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Openbus.AzureServiceBus.Message;

namespace Openbus.AzureServiceBus.Configuration
{
    public abstract class MessageConfiguration
    {
        public Type MessageType { get; set; }
    }

    public class MessageConfiguration<TMessage> : MessageConfiguration where TMessage : IMessage
    {
        public IList<RetryConfiguration> RetryConfigurations = new List<RetryConfiguration>();

        public MessageConfiguration()
        {
            MessageType = typeof(TMessage);
        }

        public bool CompleteOnValidationError { get; set; } = true;
        public bool CompleteOnConvertionError { get; set; } = true;
        public Type StateType { get; set; }
    }
}