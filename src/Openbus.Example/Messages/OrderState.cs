using System;
using Openbus.AzureServiceBus.Message;

namespace Openbus.Example.Messages
{
    public class OrderState : IState
    {
        public DateTime? NotificationSent { get; set; }
    }
}