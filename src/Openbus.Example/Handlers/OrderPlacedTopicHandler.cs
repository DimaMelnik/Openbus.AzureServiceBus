using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Pipeline;
using Openbus.Example.Messages;
using Openbus.Example.Transport;

namespace Openbus.Example.Handlers
{
    public class OrderPlacedTopicHandler : IMessageHandler<IMyTopic, OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedTopicHandler> _logger;

        public OrderPlacedTopicHandler(ILogger<OrderPlacedTopicHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(OrderPlacedEvent message, ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Order placed received from topic {message.Id}");
        }
    }
}