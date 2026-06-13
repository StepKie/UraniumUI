namespace UraniumUI.Theming;
public static class DynamicTint
{
    private static readonly BindableProperty BaseBackgroundColorProperty = BindableProperty.CreateAttached(
        "BaseBackgroundColor",
        typeof(Color),
        typeof(DynamicTint),
        default(Color));

    private static readonly BindableProperty IsApplyingTintProperty = BindableProperty.CreateAttached(
        "IsApplyingTint",
        typeof(bool),
        typeof(DynamicTint),
        false);

    private static readonly BindableProperty IsTrackingBackgroundColorProperty = BindableProperty.CreateAttached(
        "IsTrackingBackgroundColor",
        typeof(bool),
        typeof(DynamicTint),
        false);

    public static readonly BindableProperty BackgroundColorOpacityProperty = BindableProperty.CreateAttached(
        "BackgroundColorOpacity",
        typeof(float),
        typeof(DynamicTint),
        defaultValue: 1f,
        propertyChanged: OnBackgroundColorOpacityChanged);

    public static float GetBackgroundColorOpacity(BindableObject view)
    {
        return (float)view.GetValue(BackgroundColorOpacityProperty);
    }

    public static void SetBackgroundColorOpacity(BindableObject view, float value)
    {
        view.SetValue(BackgroundColorOpacityProperty, value);
    }
    
    private static void OnBackgroundColorOpacityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view || newValue is not float dynamicTintOpacity)
        {
            return;
        }

        EnsureBackgroundTracking(view);
        ApplyTint(view, dynamicTintOpacity);
    }

    private static void EnsureBackgroundTracking(View view)
    {
        if ((bool)view.GetValue(IsTrackingBackgroundColorProperty))
        {
            return;
        }

        view.SetValue(IsTrackingBackgroundColorProperty, true);
        view.SetValue(BaseBackgroundColorProperty, view.BackgroundColor);
        view.PropertyChanged += OnViewPropertyChanged;
    }

    private static void OnViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not View view || e.PropertyName != nameof(VisualElement.BackgroundColor))
        {
            return;
        }

        if ((bool)view.GetValue(IsApplyingTintProperty))
        {
            return;
        }

        view.SetValue(BaseBackgroundColorProperty, view.BackgroundColor);
        ApplyTint(view, GetBackgroundColorOpacity(view));
    }

    private static void ApplyTint(View view, float dynamicTintOpacity)
    {
        var baseBackgroundColor = view.GetValue(BaseBackgroundColorProperty) as Color ?? view.BackgroundColor;

        if (baseBackgroundColor is null)
        {
            return;
        }

        var tintedColor = IsTransparentWithoutTintSource(baseBackgroundColor)
            ? baseBackgroundColor
            : baseBackgroundColor.WithAlpha(dynamicTintOpacity);

        view.SetValue(IsApplyingTintProperty, true);
        view.BackgroundColor = tintedColor;
        view.SetValue(IsApplyingTintProperty, false);
    }

    private static bool IsTransparentWithoutTintSource(Color color)
    {
        return color == Colors.Transparent
            || (color.Alpha == 0 && color.Red == 0 && color.Green == 0 && color.Blue == 0);
    }
}
