namespace UraniumUI.Validations;

public sealed class FormValidationResult
{
    private FormValidationResult(bool isValid, IEnumerable<FormValidationError> errors)
    {
        Errors = errors?.Where(x => x is not null).ToArray() ?? Array.Empty<FormValidationError>();
        IsValid = isValid && Errors.Count == 0;
    }

    public bool IsValid { get; }

    public IReadOnlyList<FormValidationError> Errors { get; }

    public static FormValidationResult Success() => new(true, Array.Empty<FormValidationError>());

    public static FormValidationResult Invalid() => new(false, Array.Empty<FormValidationError>());

    public static FormValidationResult Error(string message) => FromErrors(new FormValidationError(message));

    public static FormValidationResult PropertyError(string propertyName, string message)
        => FromErrors(new FormValidationError(message, propertyName));

    public static FormValidationResult FromErrors(params FormValidationError[] errors) => FromErrors((IEnumerable<FormValidationError>)errors);

    public static FormValidationResult FromErrors(IEnumerable<FormValidationError> errors) => new(false, errors);
}
