using Microsoft.Extensions.Options;

namespace UraniumUI.Options;

internal static class AccessibilityOptionsProvider
{
    private static readonly UraniumUIAccessibilityOptions FallbackOptions = new();

    public static UraniumUIAccessibilityOptions Get()
    {
        try
        {
            return UraniumServiceProvider.GetService<IOptions<UraniumUIAccessibilityOptions>>()?.Value ?? FallbackOptions;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NullReferenceException or ObjectDisposedException)
        {
            return FallbackOptions;
        }
    }
}
