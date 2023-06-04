using System.Threading.Tasks;
using Openbus.AzureServiceBus.Message;

namespace Openbus.AzureServiceBus.Validator
{
    public interface IMessageValidator<T> where T : IMessage
    {
        Task<ValidationResult> Validate(T message);
    }
}