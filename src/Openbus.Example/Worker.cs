using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Transport;
using Openbus.Example.Messages;
using Openbus.Example.Transport;

namespace Openbus.Example
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        private int _executedCount;

        public Worker(ILogger<Worker> logger,
            IServiceProvider serviceProvider)
            
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _executedCount = 0;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scoped = _serviceProvider.CreateScope())
            {
                var serviceBusMessageSenderQueue = scoped.ServiceProvider.GetRequiredService<IServiceBusMessageSender<IMyQueue>>();
                var serviceBusMessageSenderSessionTopic =
                    scoped.ServiceProvider.GetRequiredService<IServiceBusMessageSender<IMySessionTopic>>();
                // var serviceBusMessageSenderTopic =
                //     scoped.ServiceProvider.GetRequiredService<IServiceBusMessageSender<IMyTopic>>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);

                    if (_executedCount < 1)
                    {
                        _logger.LogInformation("Worker send message (Queue) at: {time}", DateTime.UtcNow);
                        await serviceBusMessageSenderQueue.SendAsync(new OrderPlacedEvent()
                        {
                            Id = "123"
                        }, stoppingToken);

                        _logger.LogInformation("Worker send message (Topic) with session at: {time}", DateTime.UtcNow);
                        await serviceBusMessageSenderSessionTopic.SendAsync(new OrderPlacedEvent
                        {
                            Id = "123"
                        }, stoppingToken);
                        
                        // _logger.LogInformation("Worker send message (Topic) at: {time}", DateTime.UtcNow);
                        // await serviceBusMessageSenderTopic.SendAsync(new OrderPlacedEvent
                        // {
                        //     Id = "123"
                        // }, stoppingToken);
                    }

                    _executedCount++;
                }
            }
        }
    }
}