using System;
using Openbus.AzureServiceBus.Message;

namespace Openbus.Example.Messages
{
    public class OrderPlacedEvent : IMessage
    {
        public string Id { get; set; }
    }
}