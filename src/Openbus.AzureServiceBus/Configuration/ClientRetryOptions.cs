using Azure.Messaging.ServiceBus;

namespace Openbus.AzureServiceBus.Configuration
{
    public class ClientRetryOptions
    {
        public ServiceBusRetryMode RetryMode { get; set; }
        public int RetryDelaySeconds { get; set; }
        public int MaxRetryDelaySeconds { get; set; }
        public int MaxRetries { get; set; }
    }
}