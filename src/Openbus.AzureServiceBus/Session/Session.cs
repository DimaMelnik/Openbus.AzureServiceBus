using System;
using System.Threading;
using System.Threading.Tasks;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Processor;

namespace Openbus.AzureServiceBus.Session
{
    public class Session : ISession
    {
        private readonly IProcessContext _context;

        public Session(IProcessContext context)
        {
            _context = context;
        }

        public async Task UpdateState<TState>(TState state, CancellationToken cancellationToken)
        {
            await _context.UpdateState(state == null ? null : BinaryData.FromObjectAsJson(state));
        }

        internal async Task<TState> GetState<TState>(CancellationToken cancellationToken)
            where TState : IState, new()
        {
            var stateData = await _context.GetStateData();
            return stateData == null ? new TState() : stateData.ToObjectFromJson<TState>();
        }
    }
}