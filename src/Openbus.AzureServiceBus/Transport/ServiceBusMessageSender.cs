using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Message;

namespace Openbus.AzureServiceBus.Transport
{
    public interface IServiceBusMessageSender<TBus>
        where TBus : IBus
    {
        Task<long> ScheduleMessageAsync<TMessage>(TMessage message,
            DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default)
            where TMessage : IMessage;

        Task<IReadOnlyList<long>> ScheduleMessagesAsync<TMessage>(TMessage[] messages,
            DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default)
            where TMessage : IMessage;

        Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : IMessage;

        Task SendAsync<TMessage>(TMessage[] messages, CancellationToken cancellationToken = default)
            where TMessage : IMessage;
    }

    public class ServiceBusMessageSender<TBus> : IServiceBusMessageSender<TBus>
        where TBus : IBus
    {
        private readonly ILogger<ServiceBusMessageSender<TBus>> _logger;
        private readonly MessageConfigurationProvider<TBus> _messageConfigurationProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusSender<TBus> _serviceBusSender;

        public ServiceBusMessageSender(
            ILogger<ServiceBusMessageSender<TBus>> logger,
            MessageConfigurationProvider<TBus> messageConfigurationProvider,
            IServiceProvider serviceProvider,
            ServiceBusSender<TBus> serviceBusSender)
        {
            _logger = logger;
            _messageConfigurationProvider = messageConfigurationProvider;
            _serviceProvider = serviceProvider;
            _serviceBusSender = serviceBusSender;
        }

        private IMessageConverter<TBus, TMessage> GetMessageConverter<TMessage>()
            where TMessage : IMessage
        {
            var messageConverter = _serviceProvider.GetService<IMessageConverter<TBus, TMessage>>();
            if (messageConverter == null)
            {
                _logger.LogError("No message converter found for message type {TBus} {MessageType}", typeof(TBus).Name, typeof(TMessage).Name);
                throw new ArgumentException(
                    $"No message converter found for message type {typeof(TBus).Name} {typeof(TMessage).Name}");
            }

            return messageConverter;
        }
        
        public async Task<long> ScheduleMessageAsync<TMessage>(TMessage message,
            DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default)
            where TMessage : IMessage
        {
            var messageConverter = GetMessageConverter<TMessage>();

            _logger.LogDebug($"Serializing message {typeof(TMessage).Name}");
            var serviceBusMessage = messageConverter.Serialize(message);
            _logger.LogDebug($"Schedule message {typeof(TMessage).Name} to {typeof(TBus).Name}");
            return await _serviceBusSender.Sender.ScheduleMessageAsync(serviceBusMessage, scheduledEnqueueTime,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<long>> ScheduleMessagesAsync<TMessage>(TMessage[] messages,
            DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default)
            where TMessage : IMessage
        {
            var messageConverter = GetMessageConverter<TMessage>();

            _logger.LogDebug($"Serializing messages {typeof(TMessage).Name}");
            var serviceBusMessages =
                messages.Select(message => messageConverter.Serialize(message));
            _logger.LogDebug($"Schedule messages {typeof(TMessage).Name} to {typeof(TBus).Name}");
            return await _serviceBusSender.Sender.ScheduleMessagesAsync(serviceBusMessages, scheduledEnqueueTime,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : IMessage
        {
            var messageConverter = GetMessageConverter<TMessage>();

            _logger.LogDebug($"Serializing message {typeof(TMessage).Name}");
            var serviceBusMessage = messageConverter.Serialize(message);
            _logger.LogDebug(
                $"Sending message {typeof(TMessage).Name} to {typeof(TBus).Name}({_serviceBusSender.Sender.EntityPath})");
            await _serviceBusSender.Sender.SendMessageAsync(serviceBusMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAsync<TMessage>(TMessage[] messages, CancellationToken cancellationToken = default)
            where TMessage : IMessage
        {
            var messageConverter = GetMessageConverter<TMessage>();

            _logger.LogDebug($"Serializing messages {typeof(TMessage).Name}");
            var serviceBusMessages =
                messages.Select(message => messageConverter.Serialize(message));
            _logger.LogDebug(
                $"Sending messages {typeof(TMessage).Name} to {typeof(TBus).Name}({_serviceBusSender.Sender.EntityPath})");
            await _serviceBusSender.Sender.SendMessagesAsync(serviceBusMessages, cancellationToken).ConfigureAwait(false);
        }
    }
}