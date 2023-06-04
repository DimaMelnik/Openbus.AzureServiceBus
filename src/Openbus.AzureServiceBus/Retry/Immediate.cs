using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Processor;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Retry
{
    public class Immediate<TBus, TMessage> : IRetryStrategy<TBus, TMessage>
        where TBus : IBus
        where TMessage : IMessage
    {
        private readonly ILogger<Immediate<TBus, TMessage>> _logger;

        public Immediate(ILogger<Immediate<TBus, TMessage>> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Retry(ServiceBusReceivedMessage message, RetryConfiguration retryConfiguration,
            IProcessContext processContext, CancellationToken cancellationToken)
        {
            if (message.DeliveryCount >= retryConfiguration.MaxRetryCount)
            {
                return false;
            }
            
            await processContext.AbandonMessageAsync(message);
            _logger.LogDebug($"Immediate retry {message.DeliveryCount+1} scheduled");
            return true;
        }
    }
}