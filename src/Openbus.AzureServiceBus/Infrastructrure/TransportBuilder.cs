using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Pipeline;
using Openbus.AzureServiceBus.Processor;
using Openbus.AzureServiceBus.Retry;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Infrastructrure
{
    public class TransportBuilder<TBus>
        where TBus : IBus
    {
        private readonly ServiceBusQueueConfiguration<TBus> _queueConfiguration;
        private readonly IServiceCollection _services;
        private readonly ServiceBusTopicConfiguration<TBus> _topicConfiguration;
        private ServiceBusClient<TBus> _client;
        private MessageConfigurationProvider<TBus> _messageConfigurationProvider;

        internal TransportBuilder(IServiceCollection services, ServiceBusTopicConfiguration<TBus> topicConfiguration)
        {
            _services = services;
            _topicConfiguration = topicConfiguration;
        }

        internal TransportBuilder(IServiceCollection services, ServiceBusQueueConfiguration<TBus> queueConfiguration)
        {
            _services = services;
            _queueConfiguration = queueConfiguration;
        }

        internal TransportBuilder<TBus> AddTopicBus()
        {
            _client = new ServiceBusClient<TBus>(_topicConfiguration.ConnectionString, GetServiceBusClientOptions(_topicConfiguration.ClientRetryOptions));
            _services.AddSingleton(_client);

            _services.AddSingleton(
                new ServiceBusSender<TBus>(_client.CreateSender(_topicConfiguration.Topic)));
            
            _services.AddScoped<IServiceBusMessageSender<TBus>, ServiceBusMessageSender<TBus>>();
            
            _messageConfigurationProvider = new MessageConfigurationProvider<TBus>();
            _services.TryAddSingleton(_messageConfigurationProvider);
            return this;
        }

        internal TransportBuilder<TBus> AddQueueBus()
        {
            _client = new ServiceBusClient<TBus>(_queueConfiguration.ConnectionString, GetServiceBusClientOptions(_queueConfiguration.ClientRetryOptions));
            _services.AddSingleton(_client);
            
            _services.AddSingleton(
                new ServiceBusSender<TBus>(_client.CreateSender(_queueConfiguration.Queue)));

            _services.AddScoped<IServiceBusMessageSender<TBus>, ServiceBusMessageSender<TBus>>();
            
            _messageConfigurationProvider = new MessageConfigurationProvider<TBus>();
            _services.TryAddSingleton(_messageConfigurationProvider);
            return this;
        }

        private ServiceBusClientOptions GetServiceBusClientOptions(ClientRetryOptions retryOptions)
        {
            if (retryOptions == null) return new();

            return new()
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = retryOptions.RetryMode,
                    Delay = TimeSpan.FromSeconds(retryOptions.RetryDelaySeconds),
                    MaxDelay = TimeSpan.FromSeconds(retryOptions.MaxRetryDelaySeconds),
                    MaxRetries = retryOptions.MaxRetries
                }
            };
        }

        public TransportBuilder<TBus> WithMessage<TMessage, TState>(
            Action<MessageConfigurator<TMessage>> messageConfigurator,
            Action<RetryConfigurator> retryConfigurator = null)
            where TMessage : IMessage
            where TState : IState, new()
        {
            WithMessageGeneral<TMessage>(m =>
            {
                m.SetStateType(typeof(TState));
                messageConfigurator(m);
            }, retryConfigurator);
            _services
                .AddTransient<IMessagePipeline<TBus, TMessage>, StatefulMessagePipeline<TBus, TMessage, TState>>();
            return this;
        }

        public TransportBuilder<TBus> WithMessage<TMessage>(
            Action<MessageConfigurator<TMessage>> messageConfigurator,
            Action<RetryConfigurator> retryConfigurator = null)
            where TMessage : IMessage
        {
            WithMessageGeneral(messageConfigurator, retryConfigurator);
            _services.AddTransient<IMessagePipeline<TBus, TMessage>, StatelessMessagePipeline<TBus, TMessage>>();
            return this;
        }

        private void WithMessageGeneral<TMessage>(
            Action<MessageConfigurator<TMessage>> messageConfigurator,
            Action<RetryConfigurator> retryConfigurator = null)
            where TMessage : IMessage
        {
            if (messageConfigurator == null) throw new ArgumentNullException(nameof(messageConfigurator));

            var messageConfiguration = new MessageConfiguration<TMessage>();

            _messageConfigurationProvider.MessageConfigurations.Add(messageConfiguration);

            messageConfigurator(new MessageConfigurator<TMessage>(messageConfiguration));

            _services.AddSingleton<IRetryStrategyFactory, RetryStrategyFactory>();
            if (retryConfigurator != null)
            {
                retryConfigurator(new RetryConfigurator(messageConfiguration.RetryConfigurations));
                _services.TryAddSingleton<Exponential<TBus, TMessage>>();
                _services.TryAddSingleton<Immediate<TBus, TMessage>>();
            }
        }


        public void AddMessageProcessor()
        {
            _services.AddSingleton<IServiceBusProcessorFactory<TBus>, ServiceBusProcessorFactory<TBus>>();
            _services.AddHostedService<MessageProcessorService<TBus>>();
        }

        public void AddSessionMessageProcessor()
        {
            _services.AddSingleton<IServiceBusProcessorFactory<TBus>, ServiceBusProcessorFactory<TBus>>();
            _services.AddHostedService<MessageSessionProcessorService<TBus>>();
        }
    }
}