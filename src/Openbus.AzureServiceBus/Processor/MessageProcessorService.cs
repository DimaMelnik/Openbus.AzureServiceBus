using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Infrastructrure;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Processor
{
    public class MessageProcessorService<TBus> : MessageProcessorServiceBase<TBus>, IAsyncDisposable
        where TBus : IBus
    {
        private readonly IServiceBusProcessorFactory<TBus> _serviceBusProcessorFactory;
        private ServiceBusProcessor _serviceBusProcessor;
        
        public MessageProcessorService(ILogger<MessageProcessorService<TBus>> logger, 
            MessageConfigurationProvider<TBus> messageConfigurationProvider, 
            IServiceBusProcessorFactory<TBus> serviceBusProcessorFactory, 
            IServiceProvider serviceProvider) : base(logger, messageConfigurationProvider, serviceProvider)
        {
            _serviceBusProcessorFactory = serviceBusProcessorFactory;
        }

        protected override async Task StartProcessorAsync(CancellationToken cancellationToken)
        {
            _serviceBusProcessor = _serviceBusProcessorFactory.GetProcessor();
            _serviceBusProcessor.ProcessMessageAsync += async processSessionMessageEventArgs =>
            {
                await MessageHandler(processSessionMessageEventArgs).ConfigureAwait(false);
            };
            
            _serviceBusProcessor.ProcessErrorAsync += async processErrorEventArgs =>
            {
                await ErrorHandler(processErrorEventArgs).ConfigureAwait(false);
            };
            await _serviceBusProcessor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);;
        }

        protected override async Task StopProcessorAsync(CancellationToken cancellationToken)
        {
            await _serviceBusProcessor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusProcessor.CloseAsync();
            await _serviceBusProcessor.DisposeAsync();
        }
    }
}