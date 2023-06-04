using System;
using System.Linq;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Processor
{
    public interface IServiceBusProcessorFactory<TBus> where TBus : IBus
    {
        ServiceBusProcessor GetProcessor();
        ServiceBusSessionProcessor GetSessionProcessor();
    }

    public class ServiceBusProcessorFactory<TBus> : IServiceBusProcessorFactory<TBus>
        where TBus : IBus
    {
        private readonly ServiceBusClient<TBus> _client;
        private readonly ServiceBusQueueConfiguration<TBus> _queueConfiguration;
        private readonly ServiceBusTopicConfiguration<TBus> _topicConfiguration;

        public ServiceBusProcessorFactory(ServiceBusClient<TBus> client,
            IOptions<ServiceBusTopicConfiguration<TBus>> topicConfiguration,
            IOptions<ServiceBusQueueConfiguration<TBus>> queueConfiguration)
        {
            _client = client;
            _topicConfiguration = topicConfiguration.Value;
            _queueConfiguration = queueConfiguration.Value;
        }

        public ServiceBusProcessor GetProcessor()
        {
            var options = new ServiceBusProcessorOptions
            {
                // By default or when AutoCompleteMessages is set to true, the processor will complete the message after executing the message handler
                // Set AutoCompleteMessages to false to [settle messages](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement#peeklock) on your own.
                // In both cases, if the message handler throws an exception without settling the message, the processor will abandon the message.
                AutoCompleteMessages = false
            };

            if (typeof(TBus).GetInterfaces().Contains(typeof(IBusTopic)))
            {
                // I can also allow for multi-threading
                if (_topicConfiguration.MaxConcurrentMessagesHandled.HasValue)
                {
                    options.MaxConcurrentCalls = _topicConfiguration.MaxConcurrentMessagesHandled.Value;
                }

                if (_topicConfiguration.PrefetchCount.HasValue)
                {
                    options.PrefetchCount = _topicConfiguration.PrefetchCount.Value;
                }

                if (_topicConfiguration.MaxAutoLockRenewalDuration.HasValue)
                {
                    options.MaxAutoLockRenewalDuration = _topicConfiguration.MaxAutoLockRenewalDuration.Value;
                }

                return _client.CreateProcessor(_topicConfiguration.Topic,
                    _topicConfiguration.Subscription,
                    options
                );
            }

            if (typeof(TBus).GetInterfaces().Contains(typeof(IBusQueue)))
            {
                // I can also allow for multi-threading
                if (_queueConfiguration.MaxConcurrentMessagesHandled.HasValue)
                {
                    options.MaxConcurrentCalls = _queueConfiguration.MaxConcurrentMessagesHandled.Value;
                }

                if (_queueConfiguration.PrefetchCount.HasValue)
                {
                    options.PrefetchCount = _queueConfiguration.PrefetchCount.Value;
                }

                if (_queueConfiguration.MaxAutoLockRenewalDuration.HasValue)
                {
                    options.MaxAutoLockRenewalDuration = _queueConfiguration.MaxAutoLockRenewalDuration.Value;
                }

                return _client.CreateProcessor(_queueConfiguration.Queue,
                    options);
            }

            throw new NotImplementedException("The type of bus is not supported");
        }

        public ServiceBusSessionProcessor GetSessionProcessor()
        {
            var options = new ServiceBusSessionProcessorOptions
            {
                // By default or when AutoCompleteMessages is set to true, the processor will complete the message after executing the message handler
                // Set AutoCompleteMessages to false to [settle messages](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement#peeklock) on your own.
                // In both cases, if the message handler throws an exception without settling the message, the processor will abandon the message.
                AutoCompleteMessages = false
            };
            if (typeof(TBus).GetInterfaces().Contains(typeof(IBusTopic)))
            {
                if (_topicConfiguration.MaxConcurrentSessionsHandled.HasValue)
                    options.MaxConcurrentSessions = _topicConfiguration.MaxConcurrentSessionsHandled.Value;

                if (_topicConfiguration.MaxConcurrentMessagesHandledPerSession.HasValue)
                {
                    options.MaxConcurrentCallsPerSession =
                        _topicConfiguration.MaxConcurrentMessagesHandledPerSession.Value;
                }

                if (_topicConfiguration.SessionIdleTimeout.HasValue)
                {
                    options.SessionIdleTimeout = _topicConfiguration.SessionIdleTimeout.Value;
                }

                if (_topicConfiguration.PrefetchCount.HasValue)
                {
                    options.PrefetchCount = _topicConfiguration.PrefetchCount.Value;
                }

                if (_topicConfiguration.MaxAutoLockRenewalDuration.HasValue)
                {
                    options.MaxAutoLockRenewalDuration = _topicConfiguration.MaxAutoLockRenewalDuration.Value;
                }

                return _client.CreateSessionProcessor(_topicConfiguration.Topic,
                    _topicConfiguration.Subscription,
                    options
                );
            }

            if (typeof(TBus).GetInterfaces().Contains(typeof(IBusQueue)))
            {
                if (_queueConfiguration.MaxConcurrentSessionsHandled.HasValue)
                    options.MaxConcurrentSessions = _queueConfiguration.MaxConcurrentSessionsHandled.Value;

                if (_queueConfiguration.MaxConcurrentMessagesHandledPerSession.HasValue)
                {
                    options.MaxConcurrentCallsPerSession =
                        _queueConfiguration.MaxConcurrentMessagesHandledPerSession.Value;
                }

                if (_queueConfiguration.SessionIdleTimeout.HasValue)
                {
                    options.SessionIdleTimeout = _queueConfiguration.SessionIdleTimeout.Value;
                }

                if (_queueConfiguration.PrefetchCount.HasValue)
                {
                    options.PrefetchCount = _queueConfiguration.PrefetchCount.Value;
                }

                if (_queueConfiguration.MaxAutoLockRenewalDuration.HasValue)
                {
                    options.MaxAutoLockRenewalDuration = _queueConfiguration.MaxAutoLockRenewalDuration.Value;
                }
                
                return _client.CreateSessionProcessor(_queueConfiguration.Queue,
                    options
                );
            }

            throw new NotImplementedException("The type of bus is not supported");
        }
    }
}