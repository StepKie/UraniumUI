namespace UraniumUI.Validations;

public interface IFormValidator
{
    Task<FormValidationResult> ValidateAsync(FormValidationContext context);
}
