using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Openbus.AzureServiceBus.Validator;
using Openbus.Example.Messages;

namespace Openbus.Example.Validators
{
    public class TestEventValidator : IMessageValidator<OrderPlacedEvent>
    {
        private readonly ILogger<TestEventValidator> _logger;

        public TestEventValidator(ILogger<TestEventValidator> logger)
        {
            _logger = logger;
        }


        public async Task<ValidationResult> Validate(OrderPlacedEvent message)
        {
            return ValidationResult.Success();
        }
    }
}