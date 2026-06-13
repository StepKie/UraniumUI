using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UraniumUI.Resources;

namespace UraniumUI.Material.Controls;

public partial class TreeView : ContentView
{
    public static DataTemplate DefaultItemTemplate = new(() =>
    {
        var label = new Label { VerticalOptions = LayoutOptions.Center };
        label.SetBinding(Label.TextProperty, new Binding("Name"));
        return label;
    });

    private readonly CollectionView rootView;
    private readonly TreeViewDataController dataController;
    private readonly ConditionalWeakTable<object, CheckStateHolder> hierarchicalCheckStates = new();
    private TreeViewNode pendingScrollAnchorNode;
    private int firstVisibleNodeIndex = -1;
    private int pendingScrollAnchorIndex = -1;
    private long checkStateVersion;
    private bool isSelectedItemsUpdating;

    public event EventHandler<object> SelectedItemChanged;

    public event EventHandler<IList> SelectedItemsChanged;

    public TreeView()
    {
        dataController = new TreeViewDataController
        {
            IsItemSelected = IsItemSelected,
            GetCheckState = GetHierarchicalCheckState,
        };

        rootView = new CollectionView
        {
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
            SelectionMode = SelectionMode.None,
            ItemsSource = dataController.VisibleNodes,
        };

        dataController.VisibleNodesChanging = CaptureScrollAnchor;
        dataController.VisibleNodesChanged = RestoreScrollAnchor;

        Content = rootView;
        ApplyItemTemplate();
        rootView.Scrolled += RootView_Scrolled;
        rootView.HandlerChanged += (_, _) => UpdatePlatformAnimationState();
    }

    internal IReadOnlyList<TreeViewNode> VisibleNodes => dataController.VisibleNodes;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (SelectedItems is INotifyCollectionChanged observableSelectedItems)
        {
            observableSelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;

            if (Handler is null)
            {
                dataController.Detach();
            }
            else
            {
                observableSelectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;
                dataController.Attach();
            }
        }
        else if (Handler is null)
        {
            dataController.Detach();
        }
        else
        {
            dataController.Attach();
        }
    }

    private BindingBase childrenBinding = new Binding("Children");
    public BindingBase ChildrenBinding
    {
        get => childrenBinding;
        set
        {
            childrenBinding = value;
            dataController.SetChildrenBinding(value);
        }
    }

    private string isExpandedPropertyName;
    public string IsExpandedPropertyName
    {
        get => isExpandedPropertyName;
        set
        {
            isExpandedPropertyName = value;
            dataController.IsExpandedPropertyName = value;
            dataController.Rebuild();
        }
    }

    private string isLeafPropertyName;
    public string IsLeafPropertyName
    {
        get => isLeafPropertyName;
        set
        {
            isLeafPropertyName = value;
            dataController.IsLeafPropertyName = value;
            dataController.Rebuild();
        }
    }

    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode), typeof(SelectionMode), typeof(TreeView), SelectionMode.None,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.OnSelectionModeChanged());

    public bool UseAnimation
    {
        get => (bool)GetValue(UseAnimationProperty);
        set => SetValue(UseAnimationProperty, value);
    }

    public static readonly BindableProperty UseAnimationProperty = BindableProperty.Create(
        nameof(UseAnimation), typeof(bool), typeof(TreeView), true,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.UpdatePlatformAnimationState());

    public bool IsBusy { get; set; }

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(TreeView),
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is TreeView tree)
            {
                tree.dataController.SetItemsSource((IEnumerable)newValue);
            }
        });

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(TreeView), default, BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.OnSelectedItemChanged(oldValue, newValue));

    public ICommand SelectedItemChangedCommand
    {
        get => (ICommand)GetValue(SelectedItemChangedCommandProperty);
        set => SetValue(SelectedItemChangedCommandProperty, value);
    }

    public static readonly BindableProperty SelectedItemChangedCommandProperty = BindableProperty.Create(
        nameof(SelectedItemChangedCommand), typeof(ICommand), typeof(TreeView));

    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems), typeof(IList), typeof(TreeView),
        defaultValueCreator: _ => new ObservableCollection<object>(),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.OnSelectedItemsChanged((IList)oldValue, (IList)newValue));

    public ICommand SelectedItemsChangedCommand
    {
        get => (ICommand)GetValue(SelectedItemsChangedCommandProperty);
        set => SetValue(SelectedItemsChangedCommandProperty, value);
    }

    public static readonly BindableProperty SelectedItemsChangedCommandProperty = BindableProperty.Create(
        nameof(SelectedItemsChangedCommand), typeof(ICommand), typeof(TreeView));

    public DataTemplate ExpanderTemplate
    {
        get => (DataTemplate)GetValue(ExpanderTemplateProperty);
        set => SetValue(ExpanderTemplateProperty, value);
    }

    public static readonly BindableProperty ExpanderTemplateProperty = BindableProperty.Create(
        nameof(ExpanderTemplate), typeof(DataTemplate), typeof(TreeView), null,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.ApplyItemTemplate());

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(TreeView),
        defaultValue: DefaultItemTemplate,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.ApplyItemTemplate());

    public ICommand LoadChildrenCommand
    {
        get => (ICommand)GetValue(LoadChildrenCommandProperty);
        set => SetValue(LoadChildrenCommandProperty, value);
    }

    public static readonly BindableProperty LoadChildrenCommandProperty = BindableProperty.Create(
        nameof(LoadChildrenCommand), typeof(ICommand), typeof(TreeView), null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is TreeView tree)
            {
                tree.dataController.LoadChildrenCommand = (ICommand)newValue;
            }
        });

    public Color SelectionColor
    {
        get => (Color)GetValue(SelectionColorProperty);
        set => SetValue(SelectionColorProperty, value);
    }

    public static readonly BindableProperty SelectionColorProperty = BindableProperty.Create(
        nameof(SelectionColor), typeof(Color), typeof(TreeView),
        ColorResource.GetColor("Secondary", "SecondaryDark", Colors.Pink));

    public Brush SelectionBrush
    {
        get => (Brush)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public static readonly BindableProperty SelectionBrushProperty = BindableProperty.Create(
        nameof(SelectionBrush), typeof(Brush), typeof(TreeView), null);

    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public static readonly BindableProperty ItemSpacingProperty = BindableProperty.Create(
        nameof(ItemSpacing), typeof(double), typeof(TreeView), 0.0,
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.ApplyItemsLayout());

    internal void OnNodeClicked(TreeViewNode node)
    {
        if (node is null || SelectionMode == SelectionMode.None)
        {
            return;
        }

        if (SelectionMode == SelectionMode.Single)
        {
            SelectedItem = Equals(SelectedItem, node.Item) ? null : node.Item;
            return;
        }

        if (SelectionMode == SelectionMode.Multiple)
        {
            if (SelectedItems.Contains(node.Item))
            {
                SelectedItems.Remove(node.Item);
            }
            else
            {
                SelectedItems.Add(node.Item);
            }
        }
    }

    internal void SetNodeSelection(TreeViewNode node, bool isSelected)
    {
        if (node is null || SelectionMode == SelectionMode.None)
        {
            return;
        }

        if (SelectionMode == SelectionMode.Single)
        {
            SelectedItem = isSelected ? node.Item : null;
            return;
        }

        if (isSelected)
        {
            if (!SelectedItems.Contains(node.Item))
            {
                SelectedItems.Add(node.Item);
            }
        }
        else if (SelectedItems.Contains(node.Item))
        {
            SelectedItems.Remove(node.Item);
        }
    }

    internal IEnumerable<TreeViewNode> GetLoadedDescendants(TreeViewNode node) => dataController.EnumerateLoadedDescendants(node);

    internal void ApplyHierarchicalCheckState(TreeViewNode node, TreeViewNodeCheckState state)
    {
        if (node is null)
        {
            return;
        }

        var version = ++checkStateVersion;
        SetBaselineCheckState(node.Item, state, version);
        SetVisualCheckState(node.Item, state, version);
        dataController.RefreshVisibleCheckStates(node);
        RefreshAncestorCheckStates(node.Parent);
    }

    private void RootView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        firstVisibleNodeIndex = e.FirstVisibleItemIndex;
    }

    private void CaptureScrollAnchor()
    {
        pendingScrollAnchorNode = null;
        pendingScrollAnchorIndex = -1;

        if (dataController.VisibleNodes.Count == 0)
        {
            return;
        }

        var index = firstVisibleNodeIndex;
        if (index < 0 || index >= dataController.VisibleNodes.Count)
        {
            index = 0;
        }

        pendingScrollAnchorNode = dataController.VisibleNodes[index];
        pendingScrollAnchorIndex = index;
        CapturePlatformScrollAnchor();
    }

    private void RestoreScrollAnchor()
    {
        var anchorNode = pendingScrollAnchorNode;
        var anchorIndex = pendingScrollAnchorIndex;

        pendingScrollAnchorNode = null;
        pendingScrollAnchorIndex = -1;

        if (anchorNode is not null)
        {
            RestorePlatformScrollAnchor(anchorNode, anchorIndex);
        }
    }

    private void ApplyItemTemplate()
    {
        if (rootView is null)
        {
            return;
        }

        rootView.ItemTemplate = new DataTemplate(() => new TreeViewNodeView(this));
    }

    private void ApplyItemsLayout()
    {
        if (rootView?.ItemsLayout is LinearItemsLayout layout)
        {
            layout.ItemSpacing = ItemSpacing;
        }
    }

    private bool IsItemSelected(object item)
    {
        return SelectionMode switch
        {
            SelectionMode.Single => Equals(SelectedItem, item),
            SelectionMode.Multiple => SelectedItems?.Contains(item) == true,
            _ => false,
        };
    }

    private TreeViewNodeCheckState GetHierarchicalCheckState(TreeViewNode node)
    {
        if (node is null)
        {
            return TreeViewNodeCheckState.Unchecked;
        }

        return GetEffectiveCheckState(node.Item, node.Parent);
    }

    private TreeViewNodeCheckState GetEffectiveCheckState(object item, TreeViewNode parent)
    {
        var baseline = GetEffectiveBaselineCheckState(item, parent);
        if (TryGetCheckStateHolder(item, out var holder) && holder.VisualVersion >= baseline.Version)
        {
            return holder.VisualState;
        }

        return baseline.State;
    }

    private void RefreshAncestorCheckStates(TreeViewNode parent)
    {
        while (parent is not null)
        {
            SetVisualCheckState(parent.Item, GetStateFromDirectChildren(parent), checkStateVersion);
            dataController.RefreshCheckStateForItem(parent.Item);
            parent = parent.Parent;
        }
    }

    private TreeViewNodeCheckState GetStateFromDirectChildren(TreeViewNode parent)
    {
        var hasChecked = false;
        var hasUnchecked = false;

        foreach (var childItem in dataController.EnumerateChildItems(parent.Item))
        {
            var childState = GetEffectiveCheckState(childItem, parent);
            if (childState == TreeViewNodeCheckState.Indeterminate)
            {
                return TreeViewNodeCheckState.Indeterminate;
            }

            hasChecked |= childState == TreeViewNodeCheckState.Checked;
            hasUnchecked |= childState == TreeViewNodeCheckState.Unchecked;

            if (hasChecked && hasUnchecked)
            {
                return TreeViewNodeCheckState.Indeterminate;
            }
        }

        if (hasChecked)
        {
            return TreeViewNodeCheckState.Checked;
        }

        if (hasUnchecked)
        {
            return TreeViewNodeCheckState.Unchecked;
        }

        return GetEffectiveCheckState(parent.Item, parent.Parent);
    }

    private (TreeViewNodeCheckState State, long Version) GetEffectiveBaselineCheckState(object item, TreeViewNode parent)
    {
        var state = TreeViewNodeCheckState.Unchecked;
        var version = 0L;

        if (TryGetCheckStateHolder(item, out var itemHolder) && itemHolder.BaselineVersion > version)
        {
            state = itemHolder.BaselineState;
            version = itemHolder.BaselineVersion;
        }

        while (parent is not null)
        {
            if (TryGetCheckStateHolder(parent.Item, out var parentHolder) && parentHolder.BaselineVersion > version)
            {
                state = parentHolder.BaselineState;
                version = parentHolder.BaselineVersion;
            }

            parent = parent.Parent;
        }

        return (state, version);
    }

    private bool TryGetCheckStateHolder(object item, out CheckStateHolder holder)
    {
        if (item is not null && hierarchicalCheckStates.TryGetValue(item, out holder))
        {
            return true;
        }

        holder = null;
        return false;
    }

    private void SetBaselineCheckState(object item, TreeViewNodeCheckState state, long version)
    {
        if (item is null)
        {
            return;
        }

        var holder = GetOrCreateCheckStateHolder(item);
        holder.BaselineState = state;
        holder.BaselineVersion = version;
    }

    private void SetVisualCheckState(object item, TreeViewNodeCheckState state, long version)
    {
        if (item is null)
        {
            return;
        }

        var holder = GetOrCreateCheckStateHolder(item);
        holder.VisualState = state;
        holder.VisualVersion = version;
    }

    private CheckStateHolder GetOrCreateCheckStateHolder(object item)
    {
        if (hierarchicalCheckStates.TryGetValue(item, out var holder))
        {
            return holder;
        }

        holder = new CheckStateHolder();
        hierarchicalCheckStates.Add(item, holder);
        return holder;
    }

    private void OnSelectionModeChanged()
    {
        dataController.RefreshAllSelection();
    }

    private void NotifySelectedItemChanged(object selectedItem)
    {
        SelectedItemChanged?.Invoke(this, selectedItem);
        SelectedItemChangedCommand?.Execute(selectedItem);
    }

    private void NotifySelectedItemsChanged(IList selectedItems)
    {
        SelectedItemsChanged?.Invoke(this, selectedItems);
        SelectedItemsChangedCommand?.Execute(selectedItems);
    }

    private void OnSelectedItemChanged(object oldValue, object newValue)
    {
        if (SelectionMode == SelectionMode.Single)
        {
            if (oldValue is not null)
            {
                dataController.RefreshSelectionForItem(oldValue);
            }

            if (newValue is not null)
            {
                dataController.RefreshSelectionForItem(newValue);
            }
        }

        NotifySelectedItemChanged(newValue);
    }

    private void OnSelectedItemsChanged(IList oldValue, IList newValue)
    {
        if (oldValue is INotifyCollectionChanged observableOld)
        {
            observableOld.CollectionChanged -= OnSelectedItemsCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged observableNew)
        {
            observableNew.CollectionChanged += OnSelectedItemsCollectionChanged;
        }

        dataController.RefreshAllSelection();
        NotifySelectedItemsChanged(newValue);
    }

    private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (isSelectedItemsUpdating || SelectionMode != SelectionMode.Multiple)
        {
            return;
        }

        isSelectedItemsUpdating = true;
        try
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    dataController.RefreshSelectionForItem(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    dataController.RefreshSelectionForItem(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        dataController.RefreshSelectionForItem(item);
                    }
                }

                if (e.NewItems is not null)
                {
                    foreach (var item in e.NewItems)
                    {
                        dataController.RefreshSelectionForItem(item);
                    }
                }
            }
            else
            {
                dataController.RefreshAllSelection();
            }
        }
        finally
        {
            isSelectedItemsUpdating = false;
        }

        NotifySelectedItemsChanged(SelectedItems);
    }

    partial void UpdatePlatformAnimationState();

    partial void CapturePlatformScrollAnchor();

    partial void RestorePlatformScrollAnchor(TreeViewNode anchorNode, int anchorIndex);

    private sealed class CheckStateHolder
    {
        public TreeViewNodeCheckState BaselineState { get; set; }

        public long BaselineVersion { get; set; }

        public TreeViewNodeCheckState VisualState { get; set; }

        public long VisualVersion { get; set; }
    }
}
