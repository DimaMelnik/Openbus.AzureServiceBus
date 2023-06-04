using System;
using Microsoft.Extensions.DependencyInjection;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Retry
{
    public class RetryStrategyFactory : IRetryStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RetryStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRetryStrategy<TBus, TMessage> GetRetryStrategyForConfiguration<TBus, TMessage>(
            RetryConfiguration retryConfiguration) where TBus : IBus where TMessage : IMessage
        {
            switch (retryConfiguration.RetryStrategy)
            {
                case RetryStrategy.Exponential:
                    return _serviceProvider.GetRequiredService<Exponential<TBus, TMessage>>();
                case RetryStrategy.Imediate:
                    return _serviceProvider.GetRequiredService<Immediate<TBus, TMessage>>();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}