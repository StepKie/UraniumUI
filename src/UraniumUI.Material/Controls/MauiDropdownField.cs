using System.Collections;
using System.Windows.Input;
using UraniumUI.Controls;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Controls;

[ContentProperty(nameof(Validations))]
public class MauiDropdownField : InputField
{
    public MauiDropdown DropdownView => Content as MauiDropdown;

    public override View Content { get; set; } = new MauiDropdown
    {
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.Fill,
        StyleClass = new List<string> { "InputField.Dropdown" },
    };

    protected StatefulContentView iconClear = new StatefulContentView
    {
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.End,
        IsVisible = false,
        Padding = new Thickness(InputField.BuiltInAttachmentLeftPadding, 0, 0, 0),
        Content = new Path
        {
            Data = UraniumShapes.X,
            Fill = ColorResource.GetColor("OnBackground", "OnBackgroundDark", Colors.DarkGray).WithAlpha(.5f),
        }
    };

    public override bool HasValue => SelectedItem != null;

    public event EventHandler<object> SelectedItemChanged;

    public MauiDropdownField()
    {
        base.RegisterForEvents();

        iconClear.TappedCommand = new Command(OnClearTapped);
        UpdateClearIconState();
        ConfigureMaterialDropDownColors();

        DropdownView.SetBinding(MauiDropdown.SelectedItemProperty, new Binding(nameof(SelectedItem), BindingMode.TwoWay, source: this));
        DropdownView.SetBinding(MauiDropdown.ItemsSourceProperty, new Binding(nameof(ItemsSource), source: this));
        DropdownView.SetBinding(MauiDropdown.IsEnabledProperty, new Binding(nameof(IsEnabled), source: this));
        DropdownView.SetBinding(MauiDropdown.FontSizeProperty, new Binding(nameof(FontSize), source: this));
        DropdownView.SetBinding(MauiDropdown.FontAutoScalingEnabledProperty, new Binding(nameof(FontAutoScalingEnabled), source: this));
        DropdownView.SetBinding(MauiDropdown.FontFamilyProperty, new Binding(nameof(FontFamily), source: this));
        DropdownView.SetBinding(MauiDropdown.FontAttributesProperty, new Binding(nameof(FontAttributes), source: this));
        DropdownView.SetBinding(MauiDropdown.TextColorProperty, new Binding(nameof(TextColor), source: this));
        DropdownView.SetBinding(MauiDropdown.HorizontalTextAlignmentProperty, new Binding(nameof(HorizontalTextAlignment), source: this));
        DropdownView.SetBinding(MauiDropdown.ItemTemplateProperty, new Binding(nameof(ItemTemplate), source: this));
        DropdownView.SetBinding(MauiDropdown.SelectedItemTemplateProperty, new Binding(nameof(SelectedItemTemplate), source: this));
        DropdownView.SetBinding(MauiDropdown.ItemDisplayBindingProperty, new Binding(nameof(ItemDisplayBinding), source: this));
    }

    private void ConfigureMaterialDropDownColors()
    {
        this.SetAppThemeColor(
            TextColorProperty,
            ColorResource.GetColor("OnBackground", Colors.DarkGray),
            ColorResource.GetColor("OnBackgroundDark", Colors.LightGray));

        DropdownView.SetAppThemeColor(
            MauiDropdown.DropDownBackgroundColorProperty,
            ColorResource.GetColor("Surface", Colors.White),
            ColorResource.GetColor("SurfaceDark", Color.FromArgb("#2C3639")));

        DropdownView.SetAppThemeColor(
            MauiDropdown.DropDownBorderColorProperty,
            ColorResource.GetColor("Outline", Colors.LightGray),
            ColorResource.GetColor("OutlineDark", Colors.Gray));

        DropdownView.SetAppThemeColor(
            MauiDropdown.SelectedItemBackgroundColorProperty,
            ColorResource.GetColor("PrimaryContainer", Colors.LightGray),
            ColorResource.GetColor("PrimaryContainerDark", Colors.DarkGray));

        DropdownView.SetAppThemeColor(
            MauiDropdown.HoveredItemBackgroundColorProperty,
            ColorResource.GetColor("SurfaceVariant", Colors.LightGray),
            ColorResource.GetColor("SurfaceVariantDark", Colors.Gray));

        DropdownView.SetAppThemeColor(
            MauiDropdown.PressedItemBackgroundColorProperty,
            ColorResource.GetColor("Primary", Colors.Gray).WithAlpha(.18f),
            ColorResource.GetColor("PrimaryDark", Colors.LightGray).WithAlpha(.24f));
    }

    protected override object GetValueForValidator()
    {
        return SelectedItem;
    }

    public override void ResetValidation()
    {
        SelectedItem = null;
        base.ResetValidation();
    }

    protected virtual void OnClearTapped(object parameter)
    {
        if (IsEnabled)
        {
            SelectedItem = null;
            DropdownView.Unfocus();
        }
    }

    protected virtual void UpdateClearIconState()
    {
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

    protected virtual void OnSelectedItemChanged()
    {
        OnPropertyChanged(nameof(SelectedItem));
        CheckAndShowValidations();

        if (AllowClear)
        {
            iconClear.IsVisible = SelectedItem != null;
        }

        UpdateState();
        SelectedItemChanged?.Invoke(this, SelectedItem);
        SelectedItemChangedCommand?.Execute(SelectedItem);
    }

    protected virtual void OnAllowClearChanged()
    {
        UpdateClearIconState();
    }

    public void Close()
    {
        DropdownView?.Close();
    }

    public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(MauiDropdownField));

    public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(MauiDropdownField),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => ((MauiDropdownField)bindable).OnSelectedItemChanged());

    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(MauiDropdownField));

    public DataTemplate SelectedItemTemplate { get => (DataTemplate)GetValue(SelectedItemTemplateProperty); set => SetValue(SelectedItemTemplateProperty, value); }

    public static readonly BindableProperty SelectedItemTemplateProperty = BindableProperty.Create(
        nameof(SelectedItemTemplate), typeof(DataTemplate), typeof(MauiDropdownField));

    public BindingBase ItemDisplayBinding { get => (BindingBase)GetValue(ItemDisplayBindingProperty); set => SetValue(ItemDisplayBindingProperty, value); }

    public static readonly BindableProperty ItemDisplayBindingProperty = BindableProperty.Create(
        nameof(ItemDisplayBinding), typeof(BindingBase), typeof(MauiDropdownField));

    public bool AllowClear { get => (bool)GetValue(AllowClearProperty); set => SetValue(AllowClearProperty, value); }

    public static readonly BindableProperty AllowClearProperty = BindableProperty.Create(
        nameof(AllowClear), typeof(bool), typeof(MauiDropdownField), false,
        propertyChanged: (bindable, oldValue, newValue) => ((MauiDropdownField)bindable).OnAllowClearChanged());

    public ICommand SelectedItemChangedCommand { get => (ICommand)GetValue(SelectedItemChangedCommandProperty); set => SetValue(SelectedItemChangedCommandProperty, value); }

    public static readonly BindableProperty SelectedItemChangedCommandProperty = BindableProperty.Create(
        nameof(SelectedItemChangedCommand), typeof(ICommand), typeof(MauiDropdownField));

    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(MauiDropdownField), MauiDropdown.TextColorProperty.DefaultValue);

    public new string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

    public static readonly new BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily), typeof(string), typeof(MauiDropdownField), MauiDropdown.FontFamilyProperty.DefaultValue);

    public new double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

    public static readonly new BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(double), typeof(MauiDropdownField), MauiDropdown.FontSizeProperty.DefaultValue);

    public new FontAttributes FontAttributes { get => (FontAttributes)GetValue(FontAttributesProperty); set => SetValue(FontAttributesProperty, value); }

    public static readonly new BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes), typeof(FontAttributes), typeof(MauiDropdownField), MauiDropdown.FontAttributesProperty.DefaultValue);

    public TextAlignment HorizontalTextAlignment { get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty); set => SetValue(HorizontalTextAlignmentProperty, value); }

    public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
        nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(MauiDropdownField), MauiDropdown.HorizontalTextAlignmentProperty.DefaultValue);
}
