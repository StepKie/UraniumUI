using System.ComponentModel;

namespace UraniumUI.Material.Controls;

public sealed class TreeViewNode : UraniumBindableObject, IDisposable
{
    private bool isExpanded;
    private bool isLeaf;
    private bool isSelected;
    private bool isLoading;
    private bool hasLoadedChildren;
    private TreeViewNodeCheckState checkState;
    private bool isDisposed;

    internal TreeViewNode(TreeViewDataController owner, object item, TreeViewNode parent, int depth)
    {
        Owner = owner;
        Item = item;
        Parent = parent;
        Depth = depth;

        isExpanded = owner.ReadIsExpanded(item);
        isLeaf = owner.ReadIsLeaf(item);
        isSelected = owner.IsItemSelected(item);
        checkState = owner.GetCheckState(this);

        if (item is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnItemPropertyChanged;
        }
    }

    internal TreeViewDataController Owner { get; }

    public object Item { get; }

    public TreeViewNode Parent { get; }

    public int Depth { get; }

    public bool IsExpanded
    {
        get => isExpanded;
        set => Owner.SetExpanded(this, value, updateItem: true);
    }

    public bool IsLeaf
    {
        get => isLeaf;
        internal set => SetProperty(ref isLeaf, value);
    }

    public bool IsSelected
    {
        get => isSelected;
        internal set => SetProperty(ref isSelected, value);
    }

    public bool IsLoading
    {
        get => isLoading;
        internal set => SetProperty(ref isLoading, value);
    }

    public bool HasLoadedChildren
    {
        get => hasLoadedChildren;
        internal set => SetProperty(ref hasLoadedChildren, value);
    }

    public TreeViewNodeCheckState CheckState
    {
        get => checkState;
        internal set => SetProperty(ref checkState, value);
    }

    internal bool SetExpandedFromController(bool value)
    {
        if (isExpanded == value)
        {
            return false;
        }

        isExpanded = value;
        OnPropertyChanged(nameof(IsExpanded));
        return true;
    }

    internal void RefreshIsLeaf()
    {
        IsLeaf = Owner.ReadIsLeaf(Item);
    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (isDisposed)
        {
            return;
        }

        if (Owner.IsExpandedProperty(e.PropertyName))
        {
            Owner.SetExpanded(this, Owner.ReadIsExpanded(Item), updateItem: false);
        }

        if (Owner.IsLeafProperty(e.PropertyName))
        {
            RefreshIsLeaf();
            if (IsLeaf && IsExpanded)
            {
                Owner.SetExpanded(this, false, updateItem: true);
            }
        }

        if (Owner.IsChildrenProperty(e.PropertyName))
        {
            Owner.RefreshChildren(this);
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;

        if (Item is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnItemPropertyChanged;
        }
    }
}
