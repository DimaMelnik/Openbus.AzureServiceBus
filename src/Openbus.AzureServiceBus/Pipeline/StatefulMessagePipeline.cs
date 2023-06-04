using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Processor;
using Openbus.AzureServiceBus.Retry;
using Openbus.AzureServiceBus.Transport;
using Openbus.AzureServiceBus.Validator;

namespace Openbus.AzureServiceBus.Pipeline
{
    internal class StatefulMessagePipeline<TBus, TMessage, TState> : MessagePipeline<TBus, TMessage>
        where TBus : IBus
        where TMessage : IMessage
        where TState : IState, new()
    {
        private readonly ILogger<MessagePipeline<TBus, TMessage>> _logger;
        private readonly IMessageHandler<TBus, TMessage, TState> _messageHandler;
        
        public StatefulMessagePipeline(ILogger<StatefulMessagePipeline<TBus, TMessage, TState>> logger,
            MessageConfigurationProvider<TBus> messageConfigurationProvider,
            IRetryStrategyFactory retryStrategyFactory,
            IMessageConverter<TBus, TMessage> messageConverter,
            IMessageHandler<TBus, TMessage, TState> messageHandler = null,
            IMessageValidator<TMessage> messageValidator = null)
            : base(logger, messageConfigurationProvider, retryStrategyFactory, messageConverter, messageValidator)
        {
            _logger = logger;
            _messageHandler = messageHandler;
        }

        protected override async Task OnDeadLetterMessage(Exception exception, CancellationToken cancellationToken)
        {
            if (_messageHandler is IOnDeadLetterMessageCallback onDeadLetterMessageCallback)
            {
                await onDeadLetterMessageCallback.OnDeadLetterMessage(exception, cancellationToken);
            }
        }

        protected override async Task ExecuteMessageHandler(TMessage deserializedMessage, ServiceBusReceivedMessage message,
            CancellationToken cancellationToken, IProcessContext processingContext)
        {
            _logger.LogDebug(
                $"Execute message handler <{typeof(TBus).Name},{typeof(TMessage).Name}, {typeof(TState).Name}>");
            var session = new Session.Session(processingContext);
            var state = await session.GetState<TState>(cancellationToken);
            await _messageHandler.Handle(deserializedMessage, state, session, message, cancellationToken).ConfigureAwait(false);
        }
    }
}