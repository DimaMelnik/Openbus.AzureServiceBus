using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Retry
{
    public interface IRetryStrategyFactory
    {
        IRetryStrategy<TBus, TMessage> GetRetryStrategyForConfiguration<TBus, TMessage>(
            RetryConfiguration retryConfiguration)
            where TMessage : IMessage
            where TBus : IBus;
    }
}