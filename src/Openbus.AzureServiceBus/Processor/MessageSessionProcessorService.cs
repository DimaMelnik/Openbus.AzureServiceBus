using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Processor
{
    public class MessageSessionProcessorService<TBus> : MessageProcessorServiceBase<TBus>, IAsyncDisposable
        where TBus : IBus
    {
        private readonly IServiceBusProcessorFactory<TBus> _serviceBusProcessorFactory;
        private ServiceBusSessionProcessor _serviceBusSessionProcessor;

        public MessageSessionProcessorService(ILogger<MessageSessionProcessorService<TBus>> logger, 
            MessageConfigurationProvider<TBus> messageConfigurationProvider, 
            IServiceBusProcessorFactory<TBus> serviceBusProcessorFactory, 
            IServiceProvider serviceProvider) : base(logger, messageConfigurationProvider, serviceProvider)
        {
            _serviceBusProcessorFactory = serviceBusProcessorFactory;
        }

        protected override async Task StartProcessorAsync(CancellationToken cancellationToken)
        {
            _serviceBusSessionProcessor = _serviceBusProcessorFactory.GetSessionProcessor();
            _serviceBusSessionProcessor.ProcessMessageAsync += async processSessionMessageEventArgs =>
            {
                await MessageHandler(processSessionMessageEventArgs).ConfigureAwait(false);
            };
            
            _serviceBusSessionProcessor.ProcessErrorAsync += async processErrorEventArgs =>
            {
                await ErrorHandler(processErrorEventArgs).ConfigureAwait(false);
            };
            await _serviceBusSessionProcessor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override async Task StopProcessorAsync(CancellationToken cancellationToken)
        {
            await _serviceBusSessionProcessor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if(_serviceBusSessionProcessor == null) return;

            await _serviceBusSessionProcessor.CloseAsync();
            await _serviceBusSessionProcessor.DisposeAsync();
        }
    }
}