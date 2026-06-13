using System.Collections;
using System.Windows.Input;
using UraniumUI.Controls;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Controls;

[ContentProperty(nameof(Validations))]
public class SelectField : InputField
{
    public Select SelectView => Content as Select;

    public override View Content { get; set; } = new SelectFieldSelect
    {
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.Fill,
        StyleClass = new List<string> { "InputField.Select" },
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

    public SelectField()
    {
        base.RegisterForEvents();

        SemanticProperties.SetDescription(iconClear, "Clear selection");
        SemanticProperties.SetHint(iconClear, "Clears the selected value.");
        iconClear.TappedCommand = new Command(OnClearTapped);
        UpdateClearIconState();
        ConfigureMaterialSelectColors();

        if (SelectView is SelectFieldSelect selectFieldSelect)
        {
            selectFieldSelect.PopupAnchor = this;
        }

        SelectView.SetBinding(Select.SelectedItemProperty, new Binding(nameof(SelectedItem), BindingMode.TwoWay, source: this));
        SelectView.SetBinding(Select.ItemsSourceProperty, new Binding(nameof(ItemsSource), source: this));
        SelectView.SetBinding(Select.IsEnabledProperty, new Binding(nameof(IsEnabled), source: this));
        SelectView.SetBinding(Select.FontSizeProperty, new Binding(nameof(FontSize), source: this));
        SelectView.SetBinding(Select.FontAutoScalingEnabledProperty, new Binding(nameof(FontAutoScalingEnabled), source: this));
        SelectView.SetBinding(Select.FontFamilyProperty, new Binding(nameof(FontFamily), source: this));
        SelectView.SetBinding(Select.FontAttributesProperty, new Binding(nameof(FontAttributes), source: this));
        SelectView.SetBinding(Select.TextColorProperty, new Binding(nameof(TextColor), source: this));
        SelectView.SetBinding(Select.HorizontalTextAlignmentProperty, new Binding(nameof(HorizontalTextAlignment), source: this));
        SelectView.SetBinding(Select.ItemTemplateProperty, new Binding(nameof(ItemTemplate), source: this));
        SelectView.SetBinding(Select.SelectedItemTemplateProperty, new Binding(nameof(SelectedItemTemplate), source: this));
    }

    private void ConfigureMaterialSelectColors()
    {
        this.SetAppThemeColor(
            TextColorProperty,
            ColorResource.GetColor("OnBackground", Colors.DarkGray),
            ColorResource.GetColor("OnBackgroundDark", Colors.LightGray));

        SelectView.SetAppThemeColor(
            Select.DropDownBackgroundColorProperty,
            ColorResource.GetColor("Surface", Colors.White),
            ColorResource.GetColor("SurfaceDark", Color.FromArgb("#2C3639")));

        SelectView.SetAppThemeColor(
            Select.DropDownBorderColorProperty,
            ColorResource.GetColor("Outline", Colors.LightGray),
            ColorResource.GetColor("OutlineDark", Colors.Gray));

        SelectView.SetAppThemeColor(
            Select.PlaceholderColorProperty,
            ColorResource.GetColor("OnBackground", Colors.DarkGray).WithAlpha(.5f),
            ColorResource.GetColor("OnBackgroundDark", Colors.LightGray).WithAlpha(.5f));

        SelectView.SetAppThemeColor(
            Select.SelectedItemBackgroundColorProperty,
            ColorResource.GetColor("PrimaryContainer", Colors.LightGray),
            ColorResource.GetColor("PrimaryContainerDark", Colors.DarkGray));

        SelectView.SetAppThemeColor(
            Select.HoveredItemBackgroundColorProperty,
            ColorResource.GetColor("SurfaceVariant", Colors.LightGray),
            ColorResource.GetColor("SurfaceVariantDark", Colors.Gray));

        SelectView.SetAppThemeColor(
            Select.PressedItemBackgroundColorProperty,
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
            SelectView.Unfocus();
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
        SelectView?.Close();
    }

    public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(SelectField));

    public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(SelectField),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => ((SelectField)bindable).OnSelectedItemChanged());

    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(SelectField));

    public DataTemplate SelectedItemTemplate { get => (DataTemplate)GetValue(SelectedItemTemplateProperty); set => SetValue(SelectedItemTemplateProperty, value); }

    public static readonly BindableProperty SelectedItemTemplateProperty = BindableProperty.Create(
        nameof(SelectedItemTemplate), typeof(DataTemplate), typeof(SelectField));

    public BindingBase ItemDisplayBinding { get => SelectView?.ItemDisplayBinding; set => SelectView.ItemDisplayBinding = value; }

    public bool AllowClear { get => (bool)GetValue(AllowClearProperty); set => SetValue(AllowClearProperty, value); }

    public static readonly BindableProperty AllowClearProperty = BindableProperty.Create(
        nameof(AllowClear), typeof(bool), typeof(SelectField), false,
        propertyChanged: (bindable, oldValue, newValue) => ((SelectField)bindable).OnAllowClearChanged());

    public ICommand SelectedItemChangedCommand { get => (ICommand)GetValue(SelectedItemChangedCommandProperty); set => SetValue(SelectedItemChangedCommandProperty, value); }

    public static readonly BindableProperty SelectedItemChangedCommandProperty = BindableProperty.Create(
        nameof(SelectedItemChangedCommand), typeof(ICommand), typeof(SelectField));

    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(SelectField), Select.TextColorProperty.DefaultValue);

    public new string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

    public static readonly new BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily), typeof(string), typeof(SelectField), Select.FontFamilyProperty.DefaultValue);

    public new double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

    public static readonly new BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(double), typeof(SelectField), Select.FontSizeProperty.DefaultValue);

    public new FontAttributes FontAttributes { get => (FontAttributes)GetValue(FontAttributesProperty); set => SetValue(FontAttributesProperty, value); }

    public static readonly new BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes), typeof(FontAttributes), typeof(SelectField), Select.FontAttributesProperty.DefaultValue);

    public TextAlignment HorizontalTextAlignment { get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty); set => SetValue(HorizontalTextAlignmentProperty, value); }

    public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
        nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(SelectField), Select.HorizontalTextAlignmentProperty.DefaultValue);

    private sealed class SelectFieldSelect : Select
    {
        public VisualElement PopupAnchor { get; set; }

        protected override VisualElement GetPopupAnchor()
        {
            return PopupAnchor ?? base.GetPopupAnchor();
        }
    }
}
