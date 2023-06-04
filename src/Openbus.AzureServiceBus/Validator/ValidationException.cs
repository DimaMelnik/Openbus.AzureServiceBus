using System;

namespace Openbus.AzureServiceBus.Validator
{
    public class ValidationException : Exception
    {
        public ValidationException(object validationFailure) :  base(validationFailure.ToString())
        {
            ValidationResult = validationFailure;
        } 

        public object ValidationResult { get; private set; }
    }
}