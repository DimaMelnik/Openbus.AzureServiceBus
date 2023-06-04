using System;
using Openbus.AzureServiceBus.Message;

namespace Openbus.AzureServiceBus.Configuration
{
    public class MessageConfigurator<TMessage>
        where TMessage : IMessage
    {
        private readonly MessageConfiguration<TMessage> _messageConfiguration;

        public MessageConfigurator(MessageConfiguration<TMessage> messageConfiguration)
        {
            _messageConfiguration = messageConfiguration;
        }

        public bool CompleteOnValidationError
        {
            set => _messageConfiguration.CompleteOnValidationError = value;
        }

        public bool CompleteOnConvertionError
        {
            set => _messageConfiguration.CompleteOnConvertionError = value;
        }

        internal void SetStateType(Type stateType)
        {
            _messageConfiguration.StateType = stateType;
        }
    }
}