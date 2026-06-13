using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;
using UraniumUI.Extensions;
using UraniumUI.Pages;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Controls;

public class Select : StatefulContentView
{
    private readonly Label selectedLabel;
    private readonly ContentView selectedContent;
    private readonly Path arrowPath;
    private readonly List<object> activeItems = new();
    private readonly List<StatefulContentView> activeItemContainers = new();
    private PopupOverlayRegistration overlayRegistration;
    private BindingBase itemDisplayBinding;
    private INotifyPropertyChanged selectedItemNotifier;
    private ScrollView activeScrollView;
    private int activeItemIndex = -1;
    private int arrowRotationVersion;
    private string generatedSemanticDescription;
    private string generatedSemanticHint;

    public Select()
    {
        selectedLabel = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
        };

        selectedContent = new ContentView
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            InputTransparent = true,
            Content = selectedLabel,
        };

        arrowPath = new Path
        {
            Data = UraniumShapes.ArrowDown,
            WidthRequest = 14,
            HeightRequest = 9,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            InputTransparent = true,
            Fill = TextColor.ToSolidColorBrush(),
        };

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Padding = new Thickness(10, 6),
            BackgroundColor = Colors.Transparent,
        };

        contentGrid.Add(selectedContent, column: 0);
        contentGrid.Add(arrowPath, column: 1);

        Content = contentGrid;

        IsFocusable = true;
        TappedCommand = new Command(Toggle);
        KeyDown += OnKeyDown;
        Unfocused += OnUnfocused;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        this.SetAppThemeColor(TextColorProperty, Colors.Black, Colors.White);
        UpdateSelectedText();
        UpdateTextStyle();
        UpdateSemanticProperties();
    }

    public bool IsDropDownOpen => overlayRegistration is not null;

    public void Open()
    {
        if (IsDropDownOpen || !IsEnabled)
        {
            return;
        }

        var popup = CreateDropDownView();
        var registration = PopupOverlay.Show(GetPopupAnchor() ?? this, popup, new PopupOverlayOptions
        {
            Width = Width,
            MaxHeight = MaxDropDownHeight,
            Margin = DropDownMargin,
        });

        if (registration is null)
        {
            ClearActiveItems();
            return;
        }

        overlayRegistration = registration;
        overlayRegistration.Closed += OnOverlayClosed;
        OnPropertyChanged(nameof(IsDropDownOpen));
        SetActiveItemIndex(GetInitialActiveItemIndex(useLastWhenNoSelection: false));
        UpdateArrowRotation(isOpen: true);
        UpdateSemanticProperties();
    }

    public void Close()
    {
        overlayRegistration?.Close();
    }

    protected virtual View CreateDropDownView()
    {
        ClearActiveItems();

        var itemsLayout = new VerticalStackLayout
        {
            Spacing = 0,
        };

        if (ItemsSource is not null)
        {
            foreach (var item in ItemsSource)
            {
                activeItems.Add(item);
                var itemContainer = CreateItemContainer(item);
                activeItemContainers.Add(itemContainer as StatefulContentView);
                itemsLayout.Add(itemContainer);
            }
        }

        activeScrollView = new ScrollView
        {
            Content = itemsLayout,
            MaximumHeightRequest = MaxDropDownHeight,
        };

        return new Border
        {
            BackgroundColor = DropDownBackgroundColor,
            Stroke = DropDownBorderColor,
            StrokeThickness = DropDownBorderThickness,
            StrokeShape = new RoundRectangle { CornerRadius = DropDownCornerRadius },
            Padding = 0,
            Content = activeScrollView,
            Shadow = DropDownShadow,
        };
    }

    protected virtual VisualElement GetPopupAnchor()
    {
        return this;
    }

    protected virtual View CreateItemContainer(object item)
    {
        var itemIndex = activeItems.Count - 1;
        var content = CreateItemView(item);
        var container = new StatefulContentView
        {
            Content = content,
            HorizontalOptions = LayoutOptions.Fill,
            IsFocusable = false,
        };

        void RestoreBackground()
        {
            UpdateItemContainerBackground(container, item, itemIndex);
        }

        RestoreBackground();

        container.HoverCommand = new Command(() => container.BackgroundColor = HoveredItemBackgroundColor);
        container.PressedCommand = new Command(() => container.BackgroundColor = PressedItemBackgroundColor);
        container.HoverExitCommand = new Command(RestoreBackground);
        container.TappedCommand = new Command(() => SelectItem(item));

        return container;
    }

    protected virtual View CreateItemView(object item)
    {
        var view = CreateTemplateView(ItemTemplate, item);

        if (view is not null)
        {
            return view;
        }

        return new Label
        {
            Text = GetTextForItem(item),
            TextColor = TextColor,
            FontSize = FontSize,
            FontFamily = FontFamily,
            FontAttributes = FontAttributes,
            FontAutoScalingEnabled = FontAutoScalingEnabled,
            HorizontalTextAlignment = HorizontalTextAlignment,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(12, 10),
        };
    }

    protected virtual View CreateSelectedItemView(object item)
    {
        return CreateTemplateView(SelectedItemTemplate ?? ItemTemplate, item);
    }

    private View CreateTemplateView(DataTemplate template, object item)
    {
        template = template is DataTemplateSelector selector
            ? selector.SelectTemplate(item, this)
            : template;

        if (template is null)
        {
            return null;
        }

        var content = template.CreateContent();
        var view = content as View;

        if (view is not null)
        {
            view.BindingContext = item;
        }

        return view;
    }

    protected virtual string GetTextForItem(object item)
    {
        if (item is null)
        {
            return null;
        }

        if (ItemDisplayBinding is not null)
        {
            return ItemDisplayBinding.GetValueOnce<object>(item)?.ToString();
        }

        return item.ToString();
    }

    protected virtual void SelectItem(object item)
    {
        SelectedItem = item;
        Close();
    }

    protected virtual void Activate()
    {
        if (IsDropDownOpen)
        {
            SelectActiveItem();
            return;
        }

        OpenFromKeyboard(useLastWhenNoSelection: false);
    }

    protected virtual void Toggle()
    {
        if (IsDropDownOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void OnKeyDown(object sender, StatefulContentViewKeyEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        e.Handled = e.Key switch
        {
            StatefulContentViewKey.Enter => HandleActionKey(),
            StatefulContentViewKey.Space => HandleActionKey(),
            StatefulContentViewKey.Escape => HandleEscapeKey(),
            StatefulContentViewKey.ArrowDown => HandleArrowKey(moveBy: 1, useLastWhenNoSelection: false),
            StatefulContentViewKey.ArrowUp => HandleArrowKey(moveBy: -1, useLastWhenNoSelection: true),
            StatefulContentViewKey.Home => HandleHomeOrEndKey(useLastItem: false),
            StatefulContentViewKey.End => HandleHomeOrEndKey(useLastItem: true),
            _ => false,
        };
    }

    private bool HandleActionKey()
    {
        Activate();
        return true;
    }

    private bool HandleEscapeKey()
    {
        if (!IsDropDownOpen)
        {
            return false;
        }

        Close();
        return true;
    }

    private bool HandleArrowKey(int moveBy, bool useLastWhenNoSelection)
    {
        if (!IsDropDownOpen)
        {
            OpenFromKeyboard(useLastWhenNoSelection);
            return true;
        }

        MoveActiveItem(moveBy);
        return true;
    }

    private bool HandleHomeOrEndKey(bool useLastItem)
    {
        if (!IsDropDownOpen)
        {
            OpenFromKeyboard(useLastItem);
            return true;
        }

        SetActiveItemIndex(useLastItem ? activeItems.Count - 1 : 0);
        return true;
    }

    private void OpenFromKeyboard(bool useLastWhenNoSelection)
    {
        if (!IsDropDownOpen)
        {
            Open();
        }

        if (IsDropDownOpen)
        {
            SetActiveItemIndex(GetInitialActiveItemIndex(useLastWhenNoSelection));
        }
    }

    private void MoveActiveItem(int moveBy)
    {
        if (activeItems.Count == 0)
        {
            return;
        }

        var currentIndex = activeItemIndex >= 0 ? activeItemIndex : GetSelectedItemIndex();

        if (currentIndex < 0)
        {
            currentIndex = moveBy > 0 ? -1 : activeItems.Count;
        }

        SetActiveItemIndex(Math.Clamp(currentIndex + moveBy, 0, activeItems.Count - 1));
    }

    private void SelectActiveItem()
    {
        if (activeItems.Count == 0)
        {
            Close();
            return;
        }

        if (activeItemIndex < 0 || activeItemIndex >= activeItems.Count)
        {
            SetActiveItemIndex(GetInitialActiveItemIndex(useLastWhenNoSelection: false));
        }

        if (activeItemIndex >= 0 && activeItemIndex < activeItems.Count)
        {
            SelectItem(activeItems[activeItemIndex]);
        }
    }

    private int GetInitialActiveItemIndex(bool useLastWhenNoSelection)
    {
        if (activeItems.Count == 0)
        {
            return -1;
        }

        var selectedIndex = GetSelectedItemIndex();

        if (selectedIndex >= 0)
        {
            return selectedIndex;
        }

        return useLastWhenNoSelection ? activeItems.Count - 1 : 0;
    }

    private int GetSelectedItemIndex()
    {
        for (var i = 0; i < activeItems.Count; i++)
        {
            if (Equals(activeItems[i], SelectedItem))
            {
                return i;
            }
        }

        return -1;
    }

    private void SetActiveItemIndex(int itemIndex)
    {
        activeItemIndex = activeItems.Count == 0 ? -1 : Math.Clamp(itemIndex, 0, activeItems.Count - 1);
        UpdateItemContainerBackgrounds();
        ScrollActiveItemIntoView();
    }

    private void UpdateItemContainerBackgrounds()
    {
        for (var i = 0; i < activeItemContainers.Count; i++)
        {
            UpdateItemContainerBackground(activeItemContainers[i], activeItems[i], i);
        }
    }

    private void UpdateItemContainerBackground(StatefulContentView container, object item, int itemIndex)
    {
        if (container is null)
        {
            return;
        }

        if (itemIndex >= 0 && itemIndex == activeItemIndex)
        {
            container.BackgroundColor = HoveredItemBackgroundColor;
            return;
        }

        container.BackgroundColor = Equals(item, SelectedItem) ? SelectedItemBackgroundColor : Colors.Transparent;
    }

    private void ScrollActiveItemIntoView()
    {
        if (activeItemIndex < 0 || activeItemIndex >= activeItemContainers.Count || activeScrollView is null)
        {
            return;
        }

        var scrollView = activeScrollView;
        var container = activeItemContainers[activeItemIndex];

        if (container is null)
        {
            return;
        }

        _ = ScrollActiveItemIntoViewAsync(scrollView, container);
    }

    private static async Task ScrollActiveItemIntoViewAsync(ScrollView scrollView, Element itemContainer)
    {
        try
        {
            await scrollView.ScrollToAsync(itemContainer, ScrollToPosition.MakeVisible, animated: false);
        }
        catch
        {
            // The popup can be detached while keyboard navigation is still updating selection.
        }
    }

    private void ClearActiveItems()
    {
        activeItems.Clear();
        activeItemContainers.Clear();
        activeScrollView = null;
        activeItemIndex = -1;
    }

    private void OnUnfocused(object sender, FocusEventArgs e)
    {
        Close();
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        PreparePopupOverlay();
    }

    private void OnUnloaded(object sender, EventArgs e)
    {
        Close();
    }

    private void PreparePopupOverlay()
    {
        try
        {
            PopupOverlay.Prepare(GetPopupAnchor() ?? this);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // The control can be loaded/unloaded while navigation is replacing the page tree.
        }
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            Close();

            if (selectedItemNotifier is not null)
            {
                selectedItemNotifier.PropertyChanged -= OnSelectedItemPropertyChanged;
                selectedItemNotifier = null;
            }
        }

        base.OnHandlerChanging(args);
    }

    protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (oldValue is INotifyCollectionChanged oldObservable)
        {
            oldObservable.CollectionChanged -= OnItemsSourceCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newObservable)
        {
            newObservable.CollectionChanged += OnItemsSourceCollectionChanged;
        }

        Close();
    }

    protected virtual void OnSelectedItemChanged(object oldValue, object newValue)
    {
        if (oldValue is INotifyPropertyChanged oldNotifier)
        {
            oldNotifier.PropertyChanged -= OnSelectedItemPropertyChanged;
        }

        selectedItemNotifier = newValue as INotifyPropertyChanged;

        if (selectedItemNotifier is not null)
        {
            selectedItemNotifier.PropertyChanged += OnSelectedItemPropertyChanged;
        }

        UpdateSelectedText();
        UpdateTextStyle();
        UpdateItemContainerBackgrounds();
        UpdateSemanticProperties();
    }

    protected virtual void OnTemplatePropertyChanged()
    {
        Close();
        UpdateSelectedText();
        UpdateSemanticProperties();
    }

    protected virtual void OnTextPropertyChanged()
    {
        UpdateSelectedText();
        UpdateTextStyle();
        UpdateSemanticProperties();
    }

    private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Close();
    }

    private void OnSelectedItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateSelectedText();
        UpdateSemanticProperties();
    }

    private void OnOverlayClosed(object sender, EventArgs e)
    {
        if (overlayRegistration is not null)
        {
            overlayRegistration.Closed -= OnOverlayClosed;
            overlayRegistration = null;
            ClearActiveItems();
            OnPropertyChanged(nameof(IsDropDownOpen));
            UpdateArrowRotation(isOpen: false);
            UpdateSemanticProperties();
        }
    }

    private void UpdateArrowRotation(bool isOpen)
    {
        if (arrowPath is null)
        {
            return;
        }

        var rotation = isOpen ? 180d : 0d;
        var animationVersion = ++arrowRotationVersion;

        if (arrowPath.Handler is null)
        {
            arrowPath.Rotation = rotation;
            return;
        }

        arrowPath.CancelAnimations();
        _ = RotateArrow(rotation, animationVersion);
    }

    private async Task RotateArrow(double rotation, int animationVersion)
    {
        try
        {
            await arrowPath.RotateTo(rotation, 120, Easing.CubicOut);
        }
        catch
        {
            // The arrow may be detached while this visual-only animation is still running.
        }
        finally
        {
            if (animationVersion == arrowRotationVersion)
            {
                arrowPath.Rotation = rotation;
            }
        }
    }

    private void UpdateSelectedText()
    {
        if (selectedContent is null || selectedLabel is null)
        {
            return;
        }

        if (SelectedItem is null)
        {
            selectedLabel.Text = Placeholder;
            selectedLabel.TextColor = PlaceholderColor;
            selectedContent.Content = selectedLabel;
            return;
        }

        var selectedItemView = CreateSelectedItemView(SelectedItem);

        if (selectedItemView is not null)
        {
            selectedContent.Content = selectedItemView;
            return;
        }

        selectedLabel.Text = GetTextForItem(SelectedItem);
        selectedLabel.TextColor = TextColor;
        selectedContent.Content = selectedLabel;
    }

    private void UpdateTextStyle()
    {
        if (selectedLabel is null || arrowPath is null)
        {
            return;
        }

        selectedLabel.FontSize = FontSize;
        selectedLabel.FontFamily = FontFamily;
        selectedLabel.FontAttributes = FontAttributes;
        selectedLabel.FontAutoScalingEnabled = FontAutoScalingEnabled;
        selectedLabel.HorizontalTextAlignment = HorizontalTextAlignment;

        arrowPath.Fill = (SelectedItem is null ? PlaceholderColor : TextColor).ToSolidColorBrush();
    }

    private void UpdateSemanticProperties()
    {
        var description = SelectedItem is null ? Placeholder : GetTextForItem(SelectedItem);

        SetGeneratedSemanticDescription(string.IsNullOrWhiteSpace(description) ? nameof(Select) : description);
        SetGeneratedSemanticHint(IsDropDownOpen
            ? "Use Up and Down arrow keys to choose an item. Press Enter to select. Press Escape to close."
            : "Press Enter or Space to open. Use Up and Down arrow keys to choose an item.");
    }

    private void SetGeneratedSemanticDescription(string description)
    {
        var currentDescription = SemanticProperties.GetDescription(this);

        if (string.IsNullOrEmpty(currentDescription) || currentDescription == generatedSemanticDescription)
        {
            SemanticProperties.SetDescription(this, description);
            generatedSemanticDescription = description;
        }
    }

    private void SetGeneratedSemanticHint(string hint)
    {
        var currentHint = SemanticProperties.GetHint(this);

        if (string.IsNullOrEmpty(currentHint) || currentHint == generatedSemanticHint)
        {
            SemanticProperties.SetHint(this, hint);
            generatedSemanticHint = hint;
        }
    }

    public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(Select),
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnItemsSourceChanged((IEnumerable)oldValue, (IEnumerable)newValue));

    public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(Select),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnSelectedItemChanged(oldValue, newValue));

    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(Select),
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTemplatePropertyChanged());

    public DataTemplate SelectedItemTemplate { get => (DataTemplate)GetValue(SelectedItemTemplateProperty); set => SetValue(SelectedItemTemplateProperty, value); }

    public static readonly BindableProperty SelectedItemTemplateProperty = BindableProperty.Create(
        nameof(SelectedItemTemplate), typeof(DataTemplate), typeof(Select),
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTemplatePropertyChanged());

    public BindingBase ItemDisplayBinding
    {
        get => itemDisplayBinding;
        set
        {
            itemDisplayBinding = value;
            OnTextPropertyChanged();
            OnPropertyChanged();
        }
    }

    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(Select),
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public Color PlaceholderColor { get => (Color)GetValue(PlaceholderColorProperty); set => SetValue(PlaceholderColorProperty, value); }

    public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(
        nameof(PlaceholderColor), typeof(Color), typeof(Select), Colors.Gray,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(Select), Colors.Black,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(double), typeof(Select), Label.FontSizeProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

    public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily), typeof(string), typeof(Select), Label.FontFamilyProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public FontAttributes FontAttributes { get => (FontAttributes)GetValue(FontAttributesProperty); set => SetValue(FontAttributesProperty, value); }

    public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes), typeof(FontAttributes), typeof(Select), Label.FontAttributesProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public bool FontAutoScalingEnabled { get => (bool)GetValue(FontAutoScalingEnabledProperty); set => SetValue(FontAutoScalingEnabledProperty, value); }

    public static readonly BindableProperty FontAutoScalingEnabledProperty = BindableProperty.Create(
        nameof(FontAutoScalingEnabled), typeof(bool), typeof(Select), Label.FontAutoScalingEnabledProperty.DefaultValue,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public TextAlignment HorizontalTextAlignment { get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty); set => SetValue(HorizontalTextAlignmentProperty, value); }

    public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
        nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(Select), TextAlignment.Start,
        propertyChanged: (bindable, oldValue, newValue) => ((Select)bindable).OnTextPropertyChanged());

    public double MaxDropDownHeight { get => (double)GetValue(MaxDropDownHeightProperty); set => SetValue(MaxDropDownHeightProperty, value); }

    public static readonly BindableProperty MaxDropDownHeightProperty = BindableProperty.Create(
        nameof(MaxDropDownHeight), typeof(double), typeof(Select), 240d);

    public Thickness DropDownMargin { get => (Thickness)GetValue(DropDownMarginProperty); set => SetValue(DropDownMarginProperty, value); }

    public static readonly BindableProperty DropDownMarginProperty = BindableProperty.Create(
        nameof(DropDownMargin), typeof(Thickness), typeof(Select), new Thickness(0, 4));

    public Color DropDownBackgroundColor { get => (Color)GetValue(DropDownBackgroundColorProperty); set => SetValue(DropDownBackgroundColorProperty, value); }

    public static readonly BindableProperty DropDownBackgroundColorProperty = BindableProperty.Create(
        nameof(DropDownBackgroundColor), typeof(Color), typeof(Select), Colors.White);

    public Color DropDownBorderColor { get => (Color)GetValue(DropDownBorderColorProperty); set => SetValue(DropDownBorderColorProperty, value); }

    public static readonly BindableProperty DropDownBorderColorProperty = BindableProperty.Create(
        nameof(DropDownBorderColor), typeof(Color), typeof(Select), Colors.LightGray);

    public double DropDownBorderThickness { get => (double)GetValue(DropDownBorderThicknessProperty); set => SetValue(DropDownBorderThicknessProperty, value); }

    public static readonly BindableProperty DropDownBorderThicknessProperty = BindableProperty.Create(
        nameof(DropDownBorderThickness), typeof(double), typeof(Select), 1d);

    public CornerRadius DropDownCornerRadius { get => (CornerRadius)GetValue(DropDownCornerRadiusProperty); set => SetValue(DropDownCornerRadiusProperty, value); }

    public static readonly BindableProperty DropDownCornerRadiusProperty = BindableProperty.Create(
        nameof(DropDownCornerRadius), typeof(CornerRadius), typeof(Select), new CornerRadius(6));

    public Shadow DropDownShadow { get => (Shadow)GetValue(DropDownShadowProperty); set => SetValue(DropDownShadowProperty, value); }

    public static readonly BindableProperty DropDownShadowProperty = BindableProperty.Create(
        nameof(DropDownShadow), typeof(Shadow), typeof(Select), new Shadow
        {
            Brush = Brush.Black,
            Opacity = .18f,
            Radius = 10,
            Offset = new Point(0, 4),
        });

    public Color SelectedItemBackgroundColor { get => (Color)GetValue(SelectedItemBackgroundColorProperty); set => SetValue(SelectedItemBackgroundColorProperty, value); }

    public static readonly BindableProperty SelectedItemBackgroundColorProperty = BindableProperty.Create(
        nameof(SelectedItemBackgroundColor), typeof(Color), typeof(Select), Colors.Transparent);

    public Color HoveredItemBackgroundColor { get => (Color)GetValue(HoveredItemBackgroundColorProperty); set => SetValue(HoveredItemBackgroundColorProperty, value); }

    public static readonly BindableProperty HoveredItemBackgroundColorProperty = BindableProperty.Create(
        nameof(HoveredItemBackgroundColor), typeof(Color), typeof(Select), Colors.Black.WithAlpha(.06f));

    public Color PressedItemBackgroundColor { get => (Color)GetValue(PressedItemBackgroundColorProperty); set => SetValue(PressedItemBackgroundColorProperty, value); }

    public static readonly BindableProperty PressedItemBackgroundColorProperty = BindableProperty.Create(
        nameof(PressedItemBackgroundColor), typeof(Color), typeof(Select), Colors.Black.WithAlpha(.12f));
}
