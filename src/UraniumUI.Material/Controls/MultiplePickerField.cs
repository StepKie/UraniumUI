using Microsoft.Maui.Layouts;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using UraniumUI.Dialogs;
using UraniumUI.Extensions;
using MaterialCheckBox = UraniumUI.Material.Controls.CheckBox;

namespace UraniumUI.Material.Controls;
public partial class MultiplePickerField : InputField
{
    public ContentView MainContentView => Content as ContentView;

    private bool isBusy;
    private bool isSelectionSyncing;
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

        BindableLayout.SetItemTemplate(layout, CreateChipTemplate());

        BindableLayout.SetItemsSource(layout, SelectedItems);

        return layout;
    }

    protected virtual DataTemplate CreateChipTemplate()
    {
        return new DataTemplate(() =>
        {
            var chip = new Chip();
            ApplyGeneratedChipTextBinding(chip);
            chip.SetBinding(Chip.IsDestroyVisibleProperty, new Binding(nameof(IsChipRemoveVisible), source: this));
            chip.SelfDestruct = false;
            chip.DestroyCommand = _destroyChipCommand;
            ApplyGeneratedChipStyle(chip);
            return chip;
        });
    }

    protected virtual void ApplyGeneratedChipTextBinding(Chip chip)
    {
        chip.RemoveBinding(Chip.TextProperty);

        if (ItemDisplayBinding is null)
        {
            chip.SetBinding(Chip.TextProperty, new Binding("."));
            return;
        }

        chip.SetBinding(Chip.TextProperty, ItemDisplayBinding.CopyAsClone());
    }

    protected virtual async Task<IEnumerable<object>> DisplayPickerPromptAsync()
    {
        var selectionSource = ItemsSource?.Cast<object>() ?? Enumerable.Empty<object>();
        var selectedItems = SelectedItems?.Cast<object>();

        if (!ShouldUseCustomCheckBoxPrompt())
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

    protected virtual bool ShouldUseCustomCheckBoxPrompt()
    {
        return ItemDisplayBinding is not null || HasCheckBoxPromptStyle();
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
            Text = GetTextForItem(item),
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

    protected virtual void RefreshGeneratedChipTextBindings()
    {
        if (chipsHolderLayout is null)
        {
            return;
        }

        foreach (var chip in chipsHolderLayout.Children.OfType<Chip>())
        {
            ApplyGeneratedChipTextBinding(chip);
        }

        RefreshChipLayout();
    }

    protected override object GetValueForValidator()
    {
        return SelectedItems;
    }

    protected virtual void OnItemsSourceSet()
    {
        if (isSelectionSyncing)
        {
            return;
        }

        if (SelectedIndexes?.Count > 0)
        {
            SyncSelectedItemsFromIndexes();
        }
        else
        {
            SyncSelectedIndexesFromItems();
        }
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

        if (!isSelectionSyncing)
        {
            SyncSelectedIndexesFromItems();
        }
    }

    protected virtual void OnSelectedIndexesSet(IList oldValue, IList newValue)
    {
        if (oldValue is INotifyCollectionChanged oldObservable)
        {
            oldObservable.CollectionChanged -= SelectedIndexesChanged;
        }

        if (newValue is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged -= SelectedIndexesChanged;
            observable.CollectionChanged += SelectedIndexesChanged;
        }

        if (!isSelectionSyncing)
        {
            SyncSelectedItemsFromIndexes();
        }
    }

    private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (isSelectionSyncing)
        {
            return;
        }

        HandleSelectedItemsChanged(sender, syncSelectedIndexes: true);
    }

    private void SelectedIndexesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (isSelectionSyncing)
        {
            return;
        }

        SyncSelectedItemsFromIndexes();
    }

    private void HandleSelectedItemsChanged(object sender, bool syncSelectedIndexes)
    {
        RefreshChipLayout();
        UpdateState();

        if (syncSelectedIndexes)
        {
            SyncSelectedIndexesFromItems();
        }

        SelectedValuesChangedCommand?.Execute(SelectedItems);
        SelectedValuesChanged?.Invoke(sender, SelectedItems);
    }

    protected virtual void OnItemDisplayBindingChanged()
    {
        RefreshGeneratedChipTextBindings();
    }

    protected virtual void SyncSelectedIndexesFromItems()
    {
        var indexes = GetIndexesForSelectedItems().ToList();

        if (SelectedIndexes is null && indexes.Count == 0)
        {
            return;
        }

        isSelectionSyncing = true;
        try
        {
            if (!TryReplaceItems(SelectedIndexes, indexes.Cast<object>()))
            {
                SelectedIndexes = new ObservableCollection<int>(indexes);
            }
        }
        finally
        {
            isSelectionSyncing = false;
        }
    }

    protected virtual void SyncSelectedItemsFromIndexes()
    {
        var items = GetItemsForSelectedIndexes().ToList();

        if (SelectedItems is null && items.Count == 0)
        {
            return;
        }

        isSelectionSyncing = true;
        try
        {
            if (!TryReplaceItems(SelectedItems, items))
            {
                SelectedItems = new ObservableCollection<object>(items);
            }
        }
        finally
        {
            isSelectionSyncing = false;
        }

        BindableLayout.SetItemsSource(chipsHolderLayout, SelectedItems);
        HandleSelectedItemsChanged(this, syncSelectedIndexes: false);
    }

    protected virtual IEnumerable<int> GetIndexesForSelectedItems()
    {
        if (ItemsSource is null || SelectedItems is null)
        {
            return Enumerable.Empty<int>();
        }

        return SelectedItems
            .Cast<object>()
            .Select(item => ItemsSource.IndexOf(item))
            .Where(index => index >= 0);
    }

    protected virtual IEnumerable<object> GetItemsForSelectedIndexes()
    {
        if (ItemsSource is null || SelectedIndexes is null)
        {
            yield break;
        }

        foreach (var value in SelectedIndexes)
        {
            if (value is not int index || index < 0 || index >= ItemsSource.Count)
            {
                continue;
            }

            yield return ItemsSource[index];
        }
    }

    private static bool TryReplaceItems(IList target, IEnumerable<object> items)
    {
        if (target is null || target.IsReadOnly || target.IsFixedSize)
        {
            return false;
        }

        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }

        return true;
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

    public BindingBase ItemDisplayBinding { get => (BindingBase)GetValue(ItemDisplayBindingProperty); set => SetValue(ItemDisplayBindingProperty, value); }

    public static readonly BindableProperty ItemDisplayBindingProperty = BindableProperty.Create(
        nameof(ItemDisplayBinding),
        typeof(BindingBase),
        typeof(MultiplePickerField),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField).OnItemDisplayBindingChanged());

    public IList SelectedIndexes { get => (IList)GetValue(SelectedIndexesProperty); set => SetValue(SelectedIndexesProperty, value); }

    public static readonly BindableProperty SelectedIndexesProperty = BindableProperty.Create(
        nameof(SelectedIndexes),
        typeof(IList),
        typeof(MultiplePickerField),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as MultiplePickerField).OnSelectedIndexesSet(oldValue as IList, newValue as IList));

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
