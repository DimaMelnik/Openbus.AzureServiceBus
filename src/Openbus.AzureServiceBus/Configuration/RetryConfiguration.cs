using System;
using Openbus.AzureServiceBus.Retry;

namespace Openbus.AzureServiceBus.Configuration
{
    public class RetryConfiguration
    {
        public Type ExceptionType;

        public RetryStrategy RetryStrategy { get; set; } = RetryStrategy.Imediate;

        public int MaxRetrySeconds { get; set; }
        public int MinRetrySeconds { get; set; }

        public int MaxRetryCount { get; set; }
    }
}