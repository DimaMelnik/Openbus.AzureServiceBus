namespace Openbus.AzureServiceBus.Validator
{
    public record ValidationResult(bool IsValid, object Failure = null)
    {
        public static ValidationResult Fail(object result = null)
        {
            return new(false, result);
        }

        public static ValidationResult Success()
        {
            return new(true);
        }
    }
}