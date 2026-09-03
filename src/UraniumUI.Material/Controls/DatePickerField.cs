using Plainer.Maui.Controls;
using System.ComponentModel;
using System.Globalization;
using UraniumUI.Controls;
using UraniumUI.Dialogs;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Controls;

[ContentProperty(nameof(Validations))]
public class DatePickerField : InputField
{
    private bool isDatePromptOpen;
    private readonly Label promptLabel;

    public DatePickerView DatePickerView { get; } = new DatePickerView
    {
        VerticalOptions = LayoutOptions.Center,
#if ANDROID
        Margin = new Thickness(16, 0),
#else
        Margin = new Thickness(10, 0),
#endif
        Opacity = 0,
    };

    public override View Content { get; set; } = new Label
    {
        VerticalOptions = LayoutOptions.Center,
        VerticalTextAlignment = TextAlignment.Center,
        HorizontalOptions = LayoutOptions.Fill,
        LineBreakMode = LineBreakMode.TailTruncation,
        Margin = new Thickness(10, 0),
        InputTransparent = false,
    };

    protected StatefulContentView iconClear = new StatefulContentView
    {
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.End,
        IsVisible = false,
        Padding = new Thickness(InputField.BuiltInAttachmentLeftPadding, 0, 0, 0),
        Content = CreateClearIconPath(null),
    };

    public override bool HasValue => Date != null;

    protected IDialogService DialogService { get; }

    public DatePickerField()
    {
        base.RegisterForEvents();
        promptLabel = (Label)Content;
        iconClear.TappedCommand = new Command(OnClearTapped);
        SetActionSemantics(iconClear, AccessibilityOptions.ClearDateDescription, AccessibilityOptions.ClearDateHint);
        SemanticProperties.SetHint(promptLabel, AccessibilityOptions.OpenDatePickerHint);
        SemanticProperties.SetHint(DatePickerView, AccessibilityOptions.OpenDatePickerHint);
        DialogService = UraniumServiceProvider.Current.GetRequiredService<IDialogService>();

        promptLabel.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await OpenDatePromptAsync())
        });

        UpdateClearIconState();
        ApplyPickerMode();
    }

    protected virtual void OnUseNativePickerChanged()
    {
        ApplyPickerMode();
    }

    private void ApplyPickerMode()
    {
        // Content is an auto-property override here, so InputField's bindable-property hooks never see the swap.
        ReleaseEvents();

        if (UseNativePicker)
        {
            DatePickerView.SetBinding(DatePicker.DateProperty, new Binding(nameof(Date), source: this, mode: BindingMode.TwoWay));
            Content = DatePickerView;
        }
        else
        {
            DatePickerView.RemoveBinding(DatePicker.DateProperty);
            Content = promptLabel;
            UpdateDateText();
            UpdateDatePickerViewDate();
        }

        UpdateDatePickerOpacity();
        RegisterForEvents();
        UpdateContentSemantics();

        // Binding refreshes ride on the dispatcher; without one (constructor in unit tests) the swap is picked up when the template binds.
        if (Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread() is not null)
        {
            OnPropertyChanged(nameof(Content));
        }
    }

    protected Label DateLabel => Content as Label;

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateClearIconState();
        UpdateDateText();
    }

    protected override object GetValueForValidator()
    {
        return Date;
    }

    protected virtual void OnClearTapped(object parameter)
    {
        if (IsEnabled)
        {
            Date = null;
        }
    }

    protected virtual async Task OpenDatePromptAsync()
    {
        if (!IsEnabled || isDatePromptOpen)
        {
            return;
        }

        try
        {
            isDatePromptOpen = true;
            Date = await DialogService.DisplayDatePromptAsync(Title, Date, MinimumDate, MaximumDate);
        }
        finally
        {
            isDatePromptOpen = false;
        }
    }

    protected virtual void OnDateChanged()
    {
        OnPropertyChanged(nameof(Date));
        CheckAndShowValidations();
        UpdateDateText();
        UpdateDatePickerViewDate();
        UpdateDatePickerOpacity();

        if (AllowClear)
        {
            iconClear.IsVisible = Date != null;
        }

        UpdateState();
    }

    private void UpdateDatePickerOpacity()
    {
        DatePickerView.Opacity = UseNativePicker && Date is not null ? 1 : 0;
    }

    protected override void OnIconChanged()
    {
        var dateLabelMargin = Content?.Margin ?? default;

        base.OnIconChanged();

        if (Icon != null && Content != null)
        {
            Content.Margin = dateLabelMargin;
        }

        if (Icon == null)
        {
            DatePickerView.Margin = new Thickness(10, 0);
        }
        else
        {
            DatePickerView.Margin = new Thickness(5, 1);
        }
    }
    protected virtual void OnAllowClearChanged()
    {
        UpdateClearIconState();
    }

    protected virtual void UpdateDateText()
    {
        // Runs from the base ctor's template apply, before promptLabel is assigned.
        if (promptLabel is not null)
        {
            promptLabel.Text = Date?.ToString(Format, CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    protected virtual void UpdateDatePickerViewDate()
    {
        if (UseNativePicker)
        {
            return; // The two-way binding owns the native picker's date; a fallback write would fight a pending null.
        }

        var date = (Date ?? GetDatePickerViewFallbackDate()).Date;

        if (MinimumDate.HasValue && date < MinimumDate.Value.Date)
        {
            date = MinimumDate.Value.Date;
        }

        if (MaximumDate.HasValue && date > MaximumDate.Value.Date)
        {
            date = MaximumDate.Value.Date;
        }

        DatePickerView.Date = date;
    }

    protected virtual DateTime GetDatePickerViewFallbackDate()
    {
        return DatePicker.DateProperty.DefaultValue is DateTime date ? date.Date : DateTime.Today;
    }

    protected virtual void UpdateClearIconState()
    {
        if (endIconsContainer is null)
        {
            return;
        }

        if (AllowClear)
        {
            if (!endIconsContainer.Contains(iconClear))
            {
                endIconsContainer.Add(iconClear);
            }
        }
        else
        {
            endIconsContainer.Remove(iconClear);
        }
    }

    public override void ResetValidation()
    {
        Date = null;
        base.ResetValidation();
    }

    public DateTime? Date { get => (DateTime?)GetValue(DateProperty); set => SetValue(DateProperty, value); }

    public static readonly BindableProperty DateProperty = BindableProperty.Create(
        nameof(Date), typeof(DateTime?), typeof(DatePickerField),
        defaultValue: null, defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as DatePickerField).OnDateChanged()
        );

    public DateTime? MaximumDate { get => (DateTime?)GetValue(MaximumDateProperty); set => SetValue(MaximumDateProperty, value); }

    public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(
         nameof(MaximumDate), typeof(DateTime?), typeof(DatePickerField),
         defaultValue: null,
         propertyChanged: (bindable, oldValue, newValue) =>
         {
             var datePickerField = bindable as DatePickerField;
             datePickerField.DatePickerView.MaximumDate = (DateTime)(newValue ?? DatePicker.MaximumDateProperty.DefaultValue);
             datePickerField.UpdateDatePickerViewDate();
         }
         );

    public DateTime? MinimumDate { get => (DateTime?)GetValue(MinimumDateProperty); set => SetValue(MinimumDateProperty, value); }

    public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(
         nameof(MinimumDate), typeof(DateTime?), typeof(DatePickerField),
         defaultValue: null,
         propertyChanged: (bindable, oldValue, newValue) =>
         {
             var datePickerField = bindable as DatePickerField;
             datePickerField.DatePickerView.MinimumDate = (DateTime)(newValue ?? DatePicker.MinimumDateProperty.DefaultValue);
             datePickerField.UpdateDatePickerViewDate();
         }
         );

    public string Format { get => (string)GetValue(FormatProperty); set => SetValue(FormatProperty, value); }

    public static readonly BindableProperty FormatProperty = BindableProperty.Create(
            nameof(Format), typeof(string), typeof(DatePickerField), DatePicker.FormatProperty.DefaultValue,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var datePickerField = bindable as DatePickerField;
                datePickerField.DatePickerView.Format = (string)newValue;
                datePickerField.UpdateDateText();
            });

    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(DatePickerField), DatePicker.TextColorProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var datePickerField = bindable as DatePickerField;
            datePickerField.DatePickerView.TextColor = (Color)newValue;
            datePickerField.promptLabel?.SetValue(Label.TextColorProperty, newValue);
        });

    public double CharacterSpacing { get => (double)GetValue(CharacterSpacingProperty); set => SetValue(CharacterSpacingProperty, value); }

    public static readonly BindableProperty CharacterSpacingProperty = BindableProperty.Create(
        nameof(CharacterSpacing), typeof(double), typeof(DatePickerField), DatePicker.CharacterSpacingProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var datePickerField = bindable as DatePickerField;
            datePickerField.DatePickerView.CharacterSpacing = (double)newValue;
            datePickerField.promptLabel?.SetValue(Label.CharacterSpacingProperty, newValue);
        });

    public FontAttributes FontAttributes { get => (FontAttributes)GetValue(FontAttributesProperty); set => SetValue(FontAttributesProperty, value); }

    public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes), typeof(FontAttributes), typeof(DatePickerField), TimePicker.FontAttributesProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var datePickerField = bindable as DatePickerField;
            datePickerField.DatePickerView.FontAttributes = (FontAttributes)newValue;
            datePickerField.promptLabel?.SetValue(Label.FontAttributesProperty, newValue);
        });

    public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

    public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily), typeof(string), typeof(DatePickerField), TimePicker.FontFamilyProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var datePickerField = bindable as DatePickerField;
            datePickerField.DatePickerView.FontFamily = (string)newValue;
            datePickerField.promptLabel?.SetValue(Label.FontFamilyProperty, newValue);
        });

    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(double), typeof(DatePickerField), TimePicker.FontSizeProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var datePickerField = bindable as DatePickerField;
            datePickerField.DatePickerView.FontSize = (double)newValue;
            datePickerField.promptLabel?.SetValue(Label.FontSizeProperty, newValue);
        });

    public bool FontAutoScalingEnabled { get => (bool)GetValue(FontAutoScalingEnabledProperty); set => SetValue(FontAutoScalingEnabledProperty, value); }

    public static readonly BindableProperty FontAutoScalingEnabledProperty = BindableProperty.Create(
        nameof(FontAutoScalingEnabled), typeof(bool), typeof(DatePickerField), TimePicker.FontAutoScalingEnabledProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var datePickerField = bindable as DatePickerField;
            datePickerField.DatePickerView.FontAutoScalingEnabled = (bool)newValue;
            datePickerField.promptLabel?.SetValue(Label.FontAutoScalingEnabledProperty, newValue);
        });
    public bool AllowClear { get => (bool)GetValue(AllowClearProperty); set => SetValue(AllowClearProperty, value); }

    public static BindableProperty AllowClearProperty = BindableProperty.Create(
        nameof(AllowClear),
        typeof(bool), typeof(DatePickerField),
        true,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as DatePickerField).OnAllowClearChanged());

    /// <summary>
    /// Whether the field embeds the platform date picker (matching TimePickerField's dialog) or
    /// shows a label that opens the calendar prompt via <see cref="IDialogService"/>.
    /// </summary>
    public bool UseNativePicker { get => (bool)GetValue(UseNativePickerProperty); set => SetValue(UseNativePickerProperty, value); }

    public static readonly BindableProperty UseNativePickerProperty = BindableProperty.Create(
        nameof(UseNativePicker),
        typeof(bool), typeof(DatePickerField),
        false,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as DatePickerField).OnUseNativePickerChanged());
}
