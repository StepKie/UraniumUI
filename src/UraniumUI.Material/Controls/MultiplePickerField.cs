using Microsoft.Maui.Layouts;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using UraniumUI.Dialogs;
using MaterialCheckBox = UraniumUI.Material.Controls.CheckBox;

namespace UraniumUI.Material.Controls;
public partial class MultiplePickerField : InputField
{
    public ContentView MainContentView => Content as ContentView;

    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        protected set
        {
            isBusy = value;
            UpdateState();
        }
    }

    public override View Content { get; set; } = new ContentView();

    public override bool HasValue { get => IsBusy || SelectedItems?.Count > 0; }

    public event EventHandler<object> SelectedValuesChanged;

    protected IDialogService DialogService { get; }

    protected FlexLayout chipsHolderLayout;

    private Command _destroyChipCommand;
    private Command _pickSelectionsCommand;

    public MultiplePickerField()
    {
        MainContentView.Content = chipsHolderLayout = CreateLayout();
        base.RegisterForEvents();

        DialogService = UraniumServiceProvider.Current.GetRequiredService<IDialogService>();

        _pickSelectionsCommand = new Command(async () =>
        {
            if (SelectedItems is null)
            {
                SelectedItems = new ObservableCollection<object>();
            }

            IsBusy = true;
            var result = await DisplayPickerPromptAsync();

            if (result != null)
            {
                SelectedItems.Clear();
                foreach (var item in result)
                {
                    SelectedItems.Add(item);
                }

                UpdateState();
            }
            IsBusy = false;
        });

        this.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = _pickSelectionsCommand
        });

        _destroyChipCommand = new Command((param) =>
        {
            if (param is Chip chip)
            {
                SelectedItems.Remove(chip.BindingContext);
                UpdateState();
            }
        });
    }

    protected FlexLayout CreateLayout()
    {
        var layout = new FlexLayout
        {
            HorizontalOptions = LayoutOptions.Start,
            AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center,
            AlignContent = Microsoft.Maui.Layouts.FlexAlignContent.Center,
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            Margin = new Thickness(4),
#if IOS || MACCATALYST
        VerticalOptions = LayoutOptions.Center,
#endif
        };

        BindableLayout.SetItemTemplate(layout, new DataTemplate(() =>
        {
            var chip = new Chip();
            chip.SetBinding(Chip.TextProperty, new Binding("."));
            chip.SetBinding(Chip.IsDestroyVisibleProperty, new Binding(nameof(IsChipRemoveVisible), source: this));
            chip.SelfDestruct = false;
            chip.DestroyCommand = _destroyChipCommand;
            ApplyGeneratedChipStyle(chip);
            return chip;
        }));

        BindableLayout.SetItemsSource(layout, SelectedItems);

        return layout;
    }

    protected virtual async Task<IEnumerable<object>> DisplayPickerPromptAsync()
    {
        var selectionSource = ItemsSource?.Cast<object>() ?? Enumerable.Empty<object>();
        var selectedItems = SelectedItems?.Cast<object>();

        if (!HasCheckBoxPromptStyle())
        {
            return await DialogService.DisplayCheckBoxPromptAsync(
                this.Title,
                selectionSource,
                selectedItems);
        }

        var checkBoxGroup = CreateCheckBoxPromptContent(selectionSource, selectedItems);
        var accepted = await DialogService.DisplayViewAsync(
            this.Title,
            CreateCheckBoxPromptView(checkBoxGroup),
            "OK",
            "Cancel");

        return accepted
            ? checkBoxGroup.Children
                .OfType<MaterialCheckBox>()
                .Where(checkBox => checkBox.IsChecked)
                .Select(checkBox => checkBox.CommandParameter)
                .ToList()
            : null;
    }

    protected virtual bool HasCheckBoxPromptStyle()
    {
        return CheckBoxColor is not null
            || CheckBoxBorderColor is not null
            || CheckBoxTextColor is not null
            || CheckBoxIconColor is not null;
    }

    protected virtual View CreateCheckBoxPromptView(VerticalStackLayout checkBoxGroup)
    {
        return new ScrollView
        {
            Content = checkBoxGroup,
            VerticalOptions = LayoutOptions.Start,
        };
    }

    protected virtual VerticalStackLayout CreateCheckBoxPromptContent(IEnumerable<object> selectionSource, IEnumerable<object> selectedItems)
    {
        var selectedItemsList = selectedItems?.ToList();
        var checkBoxGroup = new VerticalStackLayout
        {
            Margin = 20,
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
        };

        foreach (var item in selectionSource)
        {
            checkBoxGroup.Add(CreateCheckBoxPromptCheckBox(item, selectedItemsList?.Contains(item) ?? false));
        }

        return checkBoxGroup;
    }

    protected virtual MaterialCheckBox CreateCheckBoxPromptCheckBox(object item, bool isChecked)
    {
        var checkBox = new MaterialCheckBox
        {
            Text = item?.ToString(),
            CommandParameter = item,
            IsChecked = isChecked,
        };

        ApplyCheckBoxPromptStyle(checkBox);

        return checkBox;
    }

    protected virtual void ApplyCheckBoxPromptStyle(MaterialCheckBox checkBox)
    {
        if (CheckBoxColor is not null)
        {
            checkBox.Color = CheckBoxColor;
        }

        if (CheckBoxBorderColor is not null)
        {
            checkBox.BorderColor = CheckBoxBorderColor;
        }

        if (CheckBoxTextColor is not null)
        {
            checkBox.TextColor = CheckBoxTextColor;
        }

        if (CheckBoxIconColor is not null)
        {
            checkBox.IconColor = CheckBoxIconColor;
        }
    }

    protected virtual void ApplyGeneratedChipStyle(Chip chip)
    {
        if (ChipBackgroundColor is not null)
        {
            chip.BackgroundColor = ChipBackgroundColor;
        }

        if (ChipTextColor is not null)
        {
            chip.TextColor = ChipTextColor;
        }

        if (ChipDestroyIconColor is not null)
        {
            chip.DestroyIconColor = ChipDestroyIconColor;
        }
    }

    protected virtual void RefreshGeneratedChipStyles()
    {
        if (chipsHolderLayout is null)
        {
            return;
        }

        foreach (var chip in chipsHolderLayout.Children.OfType<Chip>())
        {
            ApplyGeneratedChipStyle(chip);
        }

        RefreshChipLayout();
    }
    
    protected override object GetValueForValidator()
    {
        return SelectedItems;
    }

    protected virtual void OnItemsSourceSet()
    {

    }

    protected virtual void OnSelectedItemsSet(IList oldValue, IList newValue)
    {
        BindableLayout.SetItemsSource(chipsHolderLayout, SelectedItems);
        RefreshChipLayout();
        UpdateState();

        if (oldValue is INotifyCollectionChanged oldObservable)
        {
            oldObservable.CollectionChanged -= SelectedItemsChanged;
        }

        if (newValue is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged -= SelectedItemsChanged;
            observable.CollectionChanged += SelectedItemsChanged;
        }
    }

    private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshChipLayout();
        UpdateState();
        SelectedValuesChangedCommand?.Execute(SelectedItems);
        SelectedValuesChanged?.Invoke(sender, SelectedItems);
    }

    protected virtual void RefreshChipLayout()
    {
        if (Dispatcher?.IsDispatchRequired ?? false)
        {
            Dispatcher.Dispatch(RefreshChipLayoutCore);
            return;
        }

        RefreshChipLayoutCore();
    }

    private void RefreshChipLayoutCore()
    {
        chipsHolderLayout?.InvalidateMeasure();
        MainContentView?.InvalidateMeasure();
        InvalidateMeasure();
    }

    public IList ItemsSource { get => (IList)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(MultiplePickerField),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField).OnItemsSourceSet());

    public IList SelectedItems { get => (IList)GetValue(SelectedItemsProperty); set => SetValue(SelectedItemsProperty, value); }

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems),
        typeof(IList),
        typeof(MultiplePickerField),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField).OnSelectedItemsSet(oldValue as IList, newValue as IList));

    public ICommand SelectedValuesChangedCommand { get => (ICommand)GetValue(SelectedValuesChangedCommandProperty); set => SetValue(SelectedValuesChangedCommandProperty, value); }

    public static readonly BindableProperty SelectedValuesChangedCommandProperty = BindableProperty.Create(
        nameof(SelectedValuesChangedCommand),
        typeof(ICommand), typeof(MultiplePickerField),
        defaultValue: null);

    public bool IsChipRemoveVisible { get => (bool)GetValue(IsChipRemoveVisibleProperty); set => SetValue(IsChipRemoveVisibleProperty, value); }

    public static readonly BindableProperty IsChipRemoveVisibleProperty = BindableProperty.Create(
        nameof(IsChipRemoveVisible),
        typeof(bool),
        typeof(MultiplePickerField),
        defaultValue: true,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField)?.RefreshChipLayout());

    public Color ChipBackgroundColor { get => (Color)GetValue(ChipBackgroundColorProperty); set => SetValue(ChipBackgroundColorProperty, value); }

    public static readonly BindableProperty ChipBackgroundColorProperty = BindableProperty.Create(
        nameof(ChipBackgroundColor),
        typeof(Color),
        typeof(MultiplePickerField),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField)?.RefreshGeneratedChipStyles());

    public Color ChipTextColor { get => (Color)GetValue(ChipTextColorProperty); set => SetValue(ChipTextColorProperty, value); }

    public static readonly BindableProperty ChipTextColorProperty = BindableProperty.Create(
        nameof(ChipTextColor),
        typeof(Color),
        typeof(MultiplePickerField),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField)?.RefreshGeneratedChipStyles());

    public Color ChipDestroyIconColor { get => (Color)GetValue(ChipDestroyIconColorProperty); set => SetValue(ChipDestroyIconColorProperty, value); }

    public static readonly BindableProperty ChipDestroyIconColorProperty = BindableProperty.Create(
        nameof(ChipDestroyIconColor),
        typeof(Color),
        typeof(MultiplePickerField),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField)?.RefreshGeneratedChipStyles());

    public Color CheckBoxColor { get => (Color)GetValue(CheckBoxColorProperty); set => SetValue(CheckBoxColorProperty, value); }

    public static readonly BindableProperty CheckBoxColorProperty = BindableProperty.Create(
        nameof(CheckBoxColor),
        typeof(Color),
        typeof(MultiplePickerField));

    public Color CheckBoxBorderColor { get => (Color)GetValue(CheckBoxBorderColorProperty); set => SetValue(CheckBoxBorderColorProperty, value); }

    public static readonly BindableProperty CheckBoxBorderColorProperty = BindableProperty.Create(
        nameof(CheckBoxBorderColor),
        typeof(Color),
        typeof(MultiplePickerField));

    public Color CheckBoxTextColor { get => (Color)GetValue(CheckBoxTextColorProperty); set => SetValue(CheckBoxTextColorProperty, value); }

    public static readonly BindableProperty CheckBoxTextColorProperty = BindableProperty.Create(
        nameof(CheckBoxTextColor),
        typeof(Color),
        typeof(MultiplePickerField));

    public Color CheckBoxIconColor { get => (Color)GetValue(CheckBoxIconColorProperty); set => SetValue(CheckBoxIconColorProperty, value); }

    public static readonly BindableProperty CheckBoxIconColorProperty = BindableProperty.Create(
        nameof(CheckBoxIconColor),
        typeof(Color),
        typeof(MultiplePickerField));
}
