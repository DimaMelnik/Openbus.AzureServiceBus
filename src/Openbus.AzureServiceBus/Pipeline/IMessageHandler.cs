using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Session;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Pipeline
{
    /// <summary>
    ///     This interface describe consumer handler
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TBus"></typeparam>
    public interface IMessageHandler<TBus, in TMessage>
        where TMessage : IMessage
        where TBus : IBus
    {
        /// <summary>
        ///     Processes the message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="serviceBusReceivedMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Handle(TMessage message, ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken cancellationToken);
    }

    public interface IMessageHandler<TBus, in TMessage, TState>
        where TMessage : IMessage
        where TState : IState
        where TBus : IBus
    {
        Task Handle(TMessage message, TState state, ISession session,
            ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken cancellationToken);
    }
}