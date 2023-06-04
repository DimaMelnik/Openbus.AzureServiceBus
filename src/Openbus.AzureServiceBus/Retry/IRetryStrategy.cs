using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Processor;

namespace Openbus.AzureServiceBus.Retry
{
    public enum RetryStrategy
    {
        Imediate,
        Exponential
    }

    public interface IRetryStrategy<TBus, TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Execute retry strategy
        /// </summary>
        /// <param name="message"></param>
        /// <param name="retryConfiguration"></param>
        /// <param name="processContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns false when retry attempts reached max</returns>
        Task<bool> Retry(ServiceBusReceivedMessage message, RetryConfiguration retryConfiguration,
            IProcessContext processContext, CancellationToken cancellationToken);
    }
}