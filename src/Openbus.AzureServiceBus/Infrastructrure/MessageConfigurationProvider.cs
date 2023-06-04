using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Infrastructrure
{
    public class MessageConfigurationProvider<TBus>
        where TBus : IBus
    {
        public List<MessageConfiguration> MessageConfigurations { get; set; } = new();

        public MessageConfiguration<TMessage> GetConfiguration<TMessage>() where TMessage : IMessage
        {
            return MessageConfigurations.FirstOrDefault(x => x.MessageType == typeof(TMessage)) as
                MessageConfiguration<TMessage>;
        }
    }
}