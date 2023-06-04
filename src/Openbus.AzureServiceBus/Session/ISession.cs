using System.Threading;
using System.Threading.Tasks;

namespace Openbus.AzureServiceBus.Session
{
    public interface ISession
    {
        Task UpdateState<TState>(TState state, CancellationToken cancellationToken);
    }
}