using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Pipeline;
using Openbus.AzureServiceBus.Session;
using Openbus.Example.Messages;
using Openbus.Example.Transport;

namespace Openbus.Example.Handlers
{
    public class OrderPlacedSessionTopicHandler : IMessageHandler<IMySessionTopic, OrderPlacedEvent,
        OrderState>
    {
        private readonly ILogger<OrderPlacedSessionTopicHandler> _logger;

        public OrderPlacedSessionTopicHandler(ILogger<OrderPlacedSessionTopicHandler> logger)
        {
            _logger = logger;
        }


        public async Task Handle(OrderPlacedEvent message, OrderState state, ISession session,
            ServiceBusReceivedMessage serviceBusReceivedMessage, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Order placed received from session topic {message.Id}");
            await Task.Delay(150);
            //Notification sent
            state.NotificationSent = DateTime.UtcNow;
            await session.UpdateState(state, CancellationToken.None);
            _logger.LogInformation($"Processing of {message.Id} completed");
        }
        
    }
}