using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Pipeline;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Processor
{
    public abstract class MessageProcessorServiceBase<TBus>
        : IHostedService 
        where TBus : IBus
    {
        private readonly ILogger<MessageProcessorServiceBase<TBus>> _logger;
        private readonly MessageConfigurationProvider<TBus> _messageConfigurationProvider;
        
        private readonly IServiceProvider _serviceProvider;

        protected MessageProcessorServiceBase(ILogger<MessageProcessorServiceBase<TBus>> logger,
            MessageConfigurationProvider<TBus> messageConfigurationProvider,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _messageConfigurationProvider = messageConfigurationProvider;
            _serviceProvider = serviceProvider;
        }

        protected abstract Task StartProcessorAsync(CancellationToken cancellationToken);
        protected abstract Task StopProcessorAsync(CancellationToken cancellationToken);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var ctx = _logger.BeginScope(new Dictionary<string, object>
            {
                { "Bus", typeof(TBus).Name }
            });
            _logger.LogInformation("Start to receive messages");
            await StartProcessorAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var ctx = _logger.BeginScope(new Dictionary<string, object>
            {
                { "Bus", typeof(TBus).Name }
            });
            _logger.LogInformation("Stop to receive messages");
            await StopProcessorAsync(cancellationToken);
            _logger.LogInformation("Processing of all current messages completed");
        }

        protected async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var context = new ProcessContext(args);
            try
            {
                await MessageHandler(args.Message, context, args.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LogErrorWithMessageDetails(exception, args.Message);
            }
        }

        protected async Task MessageHandler(ProcessSessionMessageEventArgs args)
        {
            var context = new ProcessContext(args);
            
            try
            {
                await MessageHandler(args.Message, context, args.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LogErrorWithMessageDetails(exception, args.Message);
            }
        }

        private void LogErrorWithMessageDetails(Exception exception, ServiceBusReceivedMessage message)
        {
            using var ctx = _logger.BeginScope(new Dictionary<string, object>
            {
                { "MessageId", message.MessageId },
                { "SessionId", message.SessionId },
                { "CorrelationId", message.CorrelationId }
            });
            
            _logger.LogError(exception, "Error processing message");
        }

        protected Task ErrorHandler(ProcessErrorEventArgs args)
        {
            using var ctx = _logger.BeginScope(new Dictionary<string, object>
            {
                { "EntityPath", args.EntityPath },
                { "FullyQualifiedNamespace", args.FullyQualifiedNamespace }
            });

            _logger.LogError(args.Exception, "");

            return Task.CompletedTask;
        }

        private IMessagePipeline GetMessagePipeline(IServiceProvider serviceProvider, Type messageType)
        {
            var messageValidatorType = typeof(IMessagePipeline<,>).MakeGenericType(typeof(TBus), messageType);
            return serviceProvider.GetService(messageValidatorType) as IMessagePipeline;
        }
        private Type GetMessageType(IServiceProvider serviceProvider, ServiceBusReceivedMessage message)
        {
            foreach (var messageType in _messageConfigurationProvider.MessageConfigurations.Select(x=>x.MessageType))
            {
                var messageConverterTypes = typeof(IMessageConverter<,>).MakeGenericType(typeof(TBus), messageType);
                
                var messageConverter = serviceProvider.GetService(messageConverterTypes);

                if (messageConverter != null && messageConverter.GetType().GetMethod(nameof(IMessageConverter<IBus, IMessage>.CanDeserialize))!
                        .Invoke(messageConverter, new object[] { message }) as bool? == true)
                    return messageType;
            }
            return null;
        }
        
        private async Task MessageHandler(ServiceBusReceivedMessage message, IProcessContext context,
            CancellationToken cancellationToken)
        {
            using var ctx = _logger.BeginScope(new Dictionary<string, object>
            {
                { "MessageId", message.MessageId },
                { "SessionId", message.SessionId },
                { "CorrelationId", message.CorrelationId }
            });
                _logger.LogDebug($"Message {message.MessageId} received");
                var serviceScope = _serviceProvider.CreateScope();
                //Find message type
                var messageType = GetMessageType(serviceScope.ServiceProvider, message);
                if (messageType == null)
                {
                    _logger.LogDebug("Message configuration is not found to process message");

                    await context.CompleteMessageAsync(message, cancellationToken).ConfigureAwait(false);
                    return;
                }

                var messagePipeline = GetMessagePipeline(serviceScope.ServiceProvider, messageType);
                await messagePipeline.Execute(message, context, cancellationToken).ConfigureAwait(false);
        }
    }
}