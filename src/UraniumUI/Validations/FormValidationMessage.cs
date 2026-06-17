using InputKit.Shared.Validations;

namespace UraniumUI.Validations;

internal sealed class FormValidationMessage : IValidation
{
    public FormValidationMessage(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public bool Validate(object value) => false;
}
