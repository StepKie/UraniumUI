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
    private bool isSelectedItemsUpdating;

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
            SelectionMode = SelectionMode.None,
            ItemsSource = dataController.VisibleNodes,
        };

        Content = rootView;
        ApplyItemTemplate();
        rootView.HandlerChanged += (_, _) => UpdatePlatformAnimationState();
    }

    internal IReadOnlyList<TreeViewNode> VisibleNodes => dataController.VisibleNodes;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (SelectedItems is INotifyCollectionChanged observableSelectedItems)
        {
            observableSelectedItems.CollectionChanged -= SelectedItemsChanged;

            if (Handler is null)
            {
                dataController.Detach();
            }
            else
            {
                observableSelectedItems.CollectionChanged += SelectedItemsChanged;
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

    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems), typeof(IList), typeof(TreeView),
        defaultValueCreator: _ => new ObservableCollection<object>(),
        propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView)?.OnSelectedItemsChanged((IList)oldValue, (IList)newValue));

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

        SetStoredCheckState(node.Item, state);
        ClearDescendantCheckStates(node.Item);
        dataController.RefreshVisibleCheckStates(node);
        RefreshAncestorCheckStates(node.Parent);
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

        if (TryGetStoredCheckState(node.Item, out var state))
        {
            return state;
        }

        return GetInheritedCheckState(node.Parent);
    }

    private TreeViewNodeCheckState GetEffectiveCheckState(object item, TreeViewNode parent)
    {
        if (TryGetStoredCheckState(item, out var state))
        {
            return state;
        }

        return GetInheritedCheckState(parent);
    }

    private TreeViewNodeCheckState GetInheritedCheckState(TreeViewNode parent)
    {
        while (parent is not null)
        {
            if (TryGetStoredCheckState(parent.Item, out var state) && state != TreeViewNodeCheckState.Indeterminate)
            {
                return state;
            }

            parent = parent.Parent;
        }

        return TreeViewNodeCheckState.Unchecked;
    }

    private void RefreshAncestorCheckStates(TreeViewNode parent)
    {
        while (parent is not null)
        {
            SetStoredCheckState(parent.Item, GetStateFromDirectChildren(parent));
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

    private void ClearDescendantCheckStates(object item)
    {
        foreach (var childItem in dataController.EnumerateChildItems(item))
        {
            RemoveStoredCheckState(childItem);
            ClearDescendantCheckStates(childItem);
        }
    }

    private bool TryGetStoredCheckState(object item, out TreeViewNodeCheckState state)
    {
        if (item is not null && hierarchicalCheckStates.TryGetValue(item, out var holder))
        {
            state = holder.State;
            return true;
        }

        state = TreeViewNodeCheckState.Unchecked;
        return false;
    }

    private void SetStoredCheckState(object item, TreeViewNodeCheckState state)
    {
        if (item is null)
        {
            return;
        }

        if (hierarchicalCheckStates.TryGetValue(item, out var holder))
        {
            holder.State = state;
            return;
        }

        hierarchicalCheckStates.Add(item, new CheckStateHolder { State = state });
    }

    private void RemoveStoredCheckState(object item)
    {
        if (item is not null)
        {
            hierarchicalCheckStates.Remove(item);
        }
    }

    private void OnSelectionModeChanged()
    {
        dataController.RefreshAllSelection();
    }

    private void OnSelectedItemChanged(object oldValue, object newValue)
    {
        if (SelectionMode != SelectionMode.Single)
        {
            return;
        }

        if (oldValue is not null)
        {
            dataController.RefreshSelectionForItem(oldValue);
        }

        if (newValue is not null)
        {
            dataController.RefreshSelectionForItem(newValue);
        }
    }

    private void OnSelectedItemsChanged(IList oldValue, IList newValue)
    {
        if (oldValue is INotifyCollectionChanged observableOld)
        {
            observableOld.CollectionChanged -= SelectedItemsChanged;
        }

        if (newValue is INotifyCollectionChanged observableNew)
        {
            observableNew.CollectionChanged += SelectedItemsChanged;
        }

        dataController.RefreshAllSelection();
    }

    private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
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
    }

    partial void UpdatePlatformAnimationState();

    private sealed class CheckStateHolder
    {
        public TreeViewNodeCheckState State { get; set; }
    }
}
