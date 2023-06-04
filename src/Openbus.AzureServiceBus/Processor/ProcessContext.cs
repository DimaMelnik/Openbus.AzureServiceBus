using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Openbus.AzureServiceBus.Processor
{
    public interface IProcessContext
    {
        ServiceBusReceivedMessage Message { get; set; }

        Task AbandonMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken=default);
        Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken=default);

        Task DeadLetterMessageAsync(ServiceBusReceivedMessage message, string deadLetterReason, CancellationToken cancellationToken=default);

        Task<BinaryData> GetStateData(CancellationToken cancellationToken=default);

        Task UpdateState(BinaryData state, CancellationToken cancellationToken=default);
    }


    public class ProcessContext : IProcessContext
    {
        private readonly ProcessMessageEventArgs _processMessageEventArgs;
        private readonly ProcessSessionMessageEventArgs _processSessionMessageEventArgs;


        public ProcessContext(ProcessMessageEventArgs processMessageEventArgs)
        {
            _processMessageEventArgs = processMessageEventArgs;
        }

        public ProcessContext(ProcessSessionMessageEventArgs processSessionMessageEventArgs)
        {
            _processSessionMessageEventArgs = processSessionMessageEventArgs;
        }

        public ServiceBusReceivedMessage Message { get; set; }

        public async Task AbandonMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken=default)
        {
            if (_processMessageEventArgs != null)
                await _processMessageEventArgs.AbandonMessageAsync(message).ConfigureAwait(false);
            else
                await _processSessionMessageEventArgs.AbandonMessageAsync(message).ConfigureAwait(false);
        }

        public async Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken=default)
        {
            if (_processMessageEventArgs != null)
                await _processMessageEventArgs.CompleteMessageAsync(message).ConfigureAwait(false);
            else
                await _processSessionMessageEventArgs.CompleteMessageAsync(message).ConfigureAwait(false);
        }

        public async Task DeadLetterMessageAsync(ServiceBusReceivedMessage message, string deadLetterReason, CancellationToken cancellationToken=default)
        {
            if (_processMessageEventArgs != null)
                await _processMessageEventArgs.DeadLetterMessageAsync(message, TrimDeadLetterReason(deadLetterReason)).ConfigureAwait(false);
            else
                await _processSessionMessageEventArgs.DeadLetterMessageAsync(message, TrimDeadLetterReason(deadLetterReason)).ConfigureAwait(false);
        }

        public async Task<BinaryData> GetStateData(CancellationToken cancellationToken=default)
        {
            if (_processSessionMessageEventArgs == null)
                throw new NotSupportedException("State is not supported for sessionles");
            return await _processSessionMessageEventArgs.GetSessionStateAsync().ConfigureAwait(false);
        }

        public async Task UpdateState(BinaryData state, CancellationToken cancellationToken=default)
        {
            if (_processSessionMessageEventArgs == null)
                throw new NotSupportedException("State is not supported for sessionles");
            await _processSessionMessageEventArgs.SetSessionStateAsync(state).ConfigureAwait(false);
        }

        private string TrimDeadLetterReason(string deadLetterReason)
        {
            // Avoid this:
            // System.ArgumentOutOfRangeException: The argument 'deadLetterReason' cannot exceed 4096 characters. (Parameter 'deadLetterReason')
            return deadLetterReason.Substring(0, Math.Min(deadLetterReason.Length, 4096));
        }
    }
}