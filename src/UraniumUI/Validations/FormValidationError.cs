namespace UraniumUI.Validations;

public sealed class FormValidationError
{
    public FormValidationError(string message)
        : this(message, Array.Empty<string>())
    {
    }

    public FormValidationError(string message, string memberName)
        : this(message, new[] { memberName })
    {
    }

    public FormValidationError(string message, IEnumerable<string> memberNames)
    {
        Message = message;
        MemberNames = memberNames?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? Array.Empty<string>();
    }

    public string Message { get; }

    public IReadOnlyList<string> MemberNames { get; }

    public bool IsFormLevel => MemberNames.Count == 0;
}
