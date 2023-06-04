using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Configuration;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Processor;
using Openbus.AzureServiceBus.Retry;
using Openbus.AzureServiceBus.Transport;
using Openbus.AzureServiceBus.Validator;

namespace Openbus.AzureServiceBus.Pipeline
{
    internal interface IMessagePipeline
    {
        Task Execute(ServiceBusReceivedMessage message, IProcessContext processContext,
            CancellationToken cancellationToken);
    }

    internal interface IMessagePipeline<TBus, TMessage> : IMessagePipeline
        where TBus : IBus
        where TMessage : IMessage
    {
    }

    internal abstract class MessagePipeline<TBus, TMessage> : IMessagePipeline<TBus, TMessage>
        where TBus : IBus
        where TMessage : IMessage
    {
        private readonly ILogger<MessagePipeline<TBus, TMessage>> _logger;
        private readonly MessageConfigurationProvider<TBus> _messageConfigurationProvider;
        private readonly IMessageValidator<TMessage> _messageValidator;
        private readonly IRetryStrategyFactory _retryStrategyFactory;
        private readonly IMessageConverter<TBus, TMessage> _messageConverter;

        public MessagePipeline(ILogger<MessagePipeline<TBus, TMessage>> logger,
            MessageConfigurationProvider<TBus> messageConfigurationProvider,
            IRetryStrategyFactory retryStrategyFactory,
            IMessageConverter<TBus, TMessage> messageConverter,
            IMessageValidator<TMessage> messageValidator = null)
        {
            _logger = logger;
            _messageValidator = messageValidator;
            _messageConfigurationProvider = messageConfigurationProvider;
            _retryStrategyFactory = retryStrategyFactory;
            _messageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
        }


        public async Task Execute(ServiceBusReceivedMessage message, IProcessContext processContext,
            CancellationToken cancellationToken)
        {
            var messageConfiguration = _messageConfigurationProvider.GetConfiguration<TMessage>();

            TMessage deserializedMessage;
            try
            {
                deserializedMessage = _messageConverter.Deserialize(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Deserialize message  {ExceptionData} {ExceptionMessage}", e.Data, e.Message);
                if (messageConfiguration.CompleteOnConvertionError)
                    await processContext.CompleteMessageAsync(message).ConfigureAwait(false);
                else
                {
                    await SendMessageToDeadLetter(message, processContext, e, cancellationToken).ConfigureAwait(false);
                }

                return;
            }

            ValidationResult validationResult = null;
            if (_messageValidator is not null)
            {
                validationResult = await _messageValidator.Validate(deserializedMessage).ConfigureAwait(false);
            }

            if (validationResult is not null && !validationResult.IsValid)
            {
                _logger.BeginScope(new Dictionary<string, object>
                {
                    { "ValidationFailure", validationResult.Failure }
                });
                _logger.LogError("Validation failed");
                if (messageConfiguration.CompleteOnValidationError)
                    await processContext.CompleteMessageAsync(message).ConfigureAwait(false);
                else
                {
                    await SendMessageToDeadLetter(message, processContext, new ValidationException(validationResult), cancellationToken).ConfigureAwait(false);
                }
                return;
            }

            try
            {
                await ExecuteMessageHandler(deserializedMessage, message, cancellationToken, processContext).ConfigureAwait(false);
            }
            catch (ServiceBusException e)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogDebug("Message handler failed");

                var retryConfiguration = GetRetryConfiguration(e, messageConfiguration.RetryConfigurations);

                if (retryConfiguration != null)
                {
                    var retryStrategy =
                        _retryStrategyFactory.GetRetryStrategyForConfiguration<TBus, TMessage>(retryConfiguration);
                    var retrySuccess = await retryStrategy.Retry(message, retryConfiguration, processContext, cancellationToken).ConfigureAwait(false);
                    if (retrySuccess)
                    {
                        return;
                    }
                }
                
                _logger.LogError(e, "Message handler failed {ExceptionData} {ExceptionMessage}" ,e.Data, e.Message);
                await SendMessageToDeadLetter(message, processContext, e, cancellationToken).ConfigureAwait(false);
                
                return;
            }

            await processContext.CompleteMessageAsync(message).ConfigureAwait(false);
            _logger.LogDebug("Completed successfully");
        }

        private async Task SendMessageToDeadLetter(ServiceBusReceivedMessage message, IProcessContext processContext, Exception e,
            CancellationToken cancellationToken)
        {
            await processContext.DeadLetterMessageAsync(message, $"Exception: {e.GetType().FullName}").ConfigureAwait(false);
            _logger.LogDebug("Message sent to dead letter");
            await OnDeadLetterMessage(e, cancellationToken).ConfigureAwait(false);
        }

        protected abstract Task OnDeadLetterMessage(Exception exception, CancellationToken cancellationToken);
        
        protected abstract Task ExecuteMessageHandler(TMessage deserializedMessage, ServiceBusReceivedMessage message,
            CancellationToken cancellationToken, IProcessContext processingContext);

        private RetryConfiguration GetRetryConfiguration(Exception e, IList<RetryConfiguration> retryConfigurations)
        {
            //First try to find retry configuration that exactly match exception
            var retryConfiguration = retryConfigurations.FirstOrDefault(x =>
                x.ExceptionType == e.GetType());

            if (retryConfiguration != null)
                return retryConfiguration;

            //Try to find retry configuration for derived exceptions 
            return retryConfigurations.FirstOrDefault(x => e.GetType().IsSubclassOf(x.ExceptionType));
        }
    }
}