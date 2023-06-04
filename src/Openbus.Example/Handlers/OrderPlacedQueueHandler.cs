using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Pipeline;
using Openbus.Example.Messages;
using Openbus.Example.Transport;

namespace Openbus.Example.Handlers
{
    public class OrderPlacedQueueHandler : IMessageHandler<IMyQueue, OrderPlacedEvent>, IOnDeadLetterMessageCallback
    {
        private readonly ILogger<OrderPlacedQueueHandler> _logger;

        public OrderPlacedQueueHandler(ILogger<OrderPlacedQueueHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(OrderPlacedEvent message, ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Order placed received from queue {message.Id}");
                        
            throw new KeyNotFoundException();
        }
        
        public async Task OnDeadLetterMessage(Exception ex, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Dead Letter Message {ex.Message}", ex.Message);
        }
    }
}