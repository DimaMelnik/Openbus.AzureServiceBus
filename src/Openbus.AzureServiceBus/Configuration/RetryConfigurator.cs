using System;
using System.Collections.Generic;
using System.Linq;
using Openbus.AzureServiceBus.Retry;

namespace Openbus.AzureServiceBus.Configuration
{
    public class RetryConfigurator
    {
        private readonly IList<RetryConfiguration> _retryConfigurations;

        public RetryConfigurator(IList<RetryConfiguration> retryConfigurations)
        {
            _retryConfigurations = retryConfigurations;
        }

        private void CheckIfNotAlredyRegistered<TException>() where TException : Exception
        {
            if (_retryConfigurations.Any(x => x.ExceptionType == typeof(TException)))
                throw new ArgumentException($"Retry for exception type {typeof(TException).Name} already registered");
        }

        public void SetExponentialRetryOn<TException>(int minRetrySeconds, int maxRetrySeconds, int maxRetry)
            where TException : Exception
        {
            CheckIfNotAlredyRegistered<TException>();

            _retryConfigurations.Add(new RetryConfiguration
            {
                ExceptionType = typeof(TException),
                MinRetrySeconds = minRetrySeconds,
                MaxRetrySeconds = maxRetrySeconds,
                MaxRetryCount = maxRetry,
                RetryStrategy = RetryStrategy.Exponential
            });
        }

        public void SetImediateRetryOn<TException>(int maxRetry)
            where TException : Exception
        {
            CheckIfNotAlredyRegistered<TException>();

            _retryConfigurations.Add(new RetryConfiguration
            {
                ExceptionType = typeof(TException),
                RetryStrategy = RetryStrategy.Imediate,
                MaxRetryCount = maxRetry
            });
        }
    }
}