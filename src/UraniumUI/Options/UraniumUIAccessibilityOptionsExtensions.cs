namespace UraniumUI.Options;

public static class UraniumUIAccessibilityOptionsExtensions
{
    public static MauiAppBuilder ConfigureUraniumUIAccessibility(this MauiAppBuilder builder, Action<UraniumUIAccessibilityOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.Configure(configure);

        return builder;
    }
}
