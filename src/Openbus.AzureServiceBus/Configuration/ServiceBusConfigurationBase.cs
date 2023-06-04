using System;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Configuration
{
    public class ServiceBusConfigurationBase<T>
        where T : IBus
    {
        public string ConnectionString { get; set; }
        public string Topic { get; set; }
        public string Subscription { get; set; }
        public int? MaxConcurrentMessagesHandled { get; set; }
        public int? MaxConcurrentMessagesHandledPerSession { get; set; }
        public int? MaxConcurrentSessionsHandled { get; set; }

        //Gets the maximum amount of time to wait for a message to be received for the currently active session. After this time has elapsed, the processor will close the session and attempt to process another session. If not specified, the TryTimeout(60sec) will be used.
        public TimeSpan? SessionIdleTimeout { get; set; }
        
        public TimeSpan? MaxAutoLockRenewalDuration { get; set; }
        public int? PrefetchCount { get; set; }

        public ClientRetryOptions ClientRetryOptions { get; set; }
    }
}