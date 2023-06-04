using System;
using System.Threading;
using System.Threading.Tasks;

namespace Openbus.AzureServiceBus.Pipeline
{
    /// <summary>
    /// This handler callback from the lib when message is sent to dead letter
    /// </summary>
    public interface IOnDeadLetterMessageCallback
    {
        Task OnDeadLetterMessage(Exception ex,
            CancellationToken cancellationToken);
    }
}