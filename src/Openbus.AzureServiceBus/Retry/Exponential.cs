using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Processor;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Retry
{
    internal class Exponential<TBus, TMessage>
        : IRetryStrategy<TBus, TMessage>
        where TBus : IBus
        where TMessage : IMessage
    {
        private readonly ILogger<Exponential<TBus, TMessage>> _logger;
        private readonly MessageConfiguration<TMessage> _messageConfiguration;
        private readonly ServiceBusSender<TBus> _serviceBusSender;

        public Exponential(
            MessageConfigurationProvider<TBus> messageConfigurationProvider,
            ILogger<Exponential<TBus, TMessage>> logger,
            ServiceBusSender<TBus> serviceBusSender)
        {
            _messageConfiguration = messageConfigurationProvider.GetConfiguration<TMessage>();
            _logger = logger;
            _serviceBusSender = serviceBusSender;
        }

        public async Task<bool> Retry(ServiceBusReceivedMessage message, RetryConfiguration retryConfiguration,
            IProcessContext processContext, CancellationToken cancellationToken)
        {
            var retryCount = 1;
            if (message.ApplicationProperties.TryGetValue("RetryCount", out var r)) retryCount = (int)r + 1;
            
            if (retryCount >= retryConfiguration.MaxRetryCount) {
                return false;
            }

            // Calculate the retry interval based on the number of retry attempts and the
            // min/max retry seconds settings in the configuration.           
            var exponentialBackoffSeconds = Math.Min(
                retryConfiguration.MaxRetrySeconds,
                Convert.ToInt32(retryConfiguration.MinRetrySeconds * Math.Pow(2, retryCount))
            );
            var scheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(exponentialBackoffSeconds);
            var delayedMessage = new ServiceBusMessage(message);

            delayedMessage.ScheduledEnqueueTime = scheduledEnqueueTimeUtc;
            delayedMessage.ApplicationProperties["RetryCount"] = retryCount;
            using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _serviceBusSender.Sender.SendMessageAsync(delayedMessage);
                await processContext.CompleteMessageAsync(message);
                ts.Complete();
            }
            _logger.LogDebug($"Retry {retryCount+1} scheduled at {scheduledEnqueueTimeUtc}.");
            return true;
        }
    }
}