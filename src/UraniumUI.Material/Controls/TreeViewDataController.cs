using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Input;

namespace UraniumUI.Material.Controls;

internal sealed class TreeViewDataController : IDisposable
{
    private readonly Dictionary<(Type Type, string PropertyName), PropertyInfo> propertyCache = new();
    private readonly Dictionary<TreeViewNode, (INotifyCollectionChanged Collection, NotifyCollectionChangedEventHandler Handler)> childSubscriptions = new();
    private IEnumerable itemsSource;
    private INotifyCollectionChanged rootCollection;
    private NotifyCollectionChangedEventHandler rootCollectionHandler;
    private string childrenPropertyName = "Children";
    private bool isDetached;
    private bool isDisposed;

    public TreeViewVisibleNodeCollection VisibleNodes { get; } = new();

    public BindingBase ChildrenBinding { get; private set; } = new Binding("Children");

    public string IsExpandedPropertyName { get; set; }

    public string IsLeafPropertyName { get; set; }

    public ICommand LoadChildrenCommand { get; set; }

    public Func<object, bool> IsItemSelected { get; set; } = _ => false;

    public Func<TreeViewNode, TreeViewNodeCheckState> GetCheckState { get; set; } = _ => TreeViewNodeCheckState.Unchecked;

    public Action VisibleNodesChanging { get; set; } = () => { };

    public Action VisibleNodesChanged { get; set; } = () => { };

    public void SetItemsSource(IEnumerable value)
    {
        if (ReferenceEquals(itemsSource, value))
        {
            return;
        }

        UnsubscribeRootCollection();
        itemsSource = value;
        if (isDetached)
        {
            VisibleNodes.Clear();
            return;
        }

        SubscribeRootCollection();
        Rebuild();
    }

    public void SetChildrenBinding(BindingBase value)
    {
        ChildrenBinding = value;
        childrenPropertyName = value is Binding binding ? binding.Path : null;
        if (!isDetached)
        {
            Rebuild();
        }
    }

    public void Rebuild()
    {
        ThrowIfDisposed();

        DisposeNodes(VisibleNodes);

        var nodes = new List<TreeViewNode>();
        foreach (var item in Enumerate(itemsSource))
        {
            AppendNodeAndExpandedDescendants(nodes, CreateNode(item, null, 0));
        }

        VisibleNodes.ReplaceWith(nodes);
    }

    public void Attach()
    {
        ThrowIfDisposed();
        isDetached = false;
        SubscribeRootCollection();
        Rebuild();
    }

    public void Detach()
    {
        isDetached = true;
        UnsubscribeRootCollection();
        DisposeNodes(VisibleNodes);
        VisibleNodes.Clear();
    }

    public void SetExpanded(TreeViewNode node, bool value, bool updateItem)
    {
        ThrowIfDisposed();

        if (value && node.IsLeaf)
        {
            if (updateItem)
            {
                WriteIsExpanded(node.Item, false);
            }
            return;
        }

        if (!node.SetExpandedFromController(value))
        {
            if (updateItem)
            {
                WriteIsExpanded(node.Item, value);
            }
            return;
        }

        if (updateItem)
        {
            WriteIsExpanded(node.Item, value);
        }

        if (value)
        {
            ExpandNode(node);
        }
        else
        {
            CollapseNode(node);
        }
    }

    public bool IsExpandedProperty(string propertyName) => IsConfiguredProperty(propertyName, IsExpandedPropertyName);

    public bool IsLeafProperty(string propertyName) => IsConfiguredProperty(propertyName, IsLeafPropertyName);

    public bool IsChildrenProperty(string propertyName) => IsConfiguredProperty(propertyName, childrenPropertyName);

    public bool ReadIsExpanded(object item)
    {
        if (string.IsNullOrWhiteSpace(IsExpandedPropertyName))
        {
            return false;
        }

        return ReadBoolean(item, IsExpandedPropertyName, false);
    }

    public bool ReadIsLeaf(object item)
    {
        if (!string.IsNullOrWhiteSpace(IsLeafPropertyName))
        {
            return ReadBoolean(item, IsLeafPropertyName, true);
        }

        return !HasChildren(item);
    }

    public void RefreshChildren(TreeViewNode node)
    {
        UnsubscribeChildren(node);
        SubscribeChildren(node);
        node.RefreshIsLeaf();

        if (node.IsLeaf && node.IsExpanded)
        {
            SetExpanded(node, false, updateItem: true);
            return;
        }

        if (node.IsExpanded)
        {
            RebuildExpandedChildren(node);
        }
    }

    public void RefreshSelectionForItem(object item)
    {
        foreach (var node in VisibleNodes)
        {
            if (Equals(node.Item, item))
            {
                node.IsSelected = IsItemSelected(node.Item);
            }
        }
    }

    public void RefreshAllSelection()
    {
        foreach (var node in VisibleNodes)
        {
            node.IsSelected = IsItemSelected(node.Item);
        }
    }

    public void RefreshCheckStateForItem(object item)
    {
        foreach (var node in VisibleNodes)
        {
            if (Equals(node.Item, item))
            {
                node.CheckState = GetCheckState(node);
            }
        }
    }

    public void RefreshVisibleCheckStates(TreeViewNode node)
    {
        if (node is null)
        {
            return;
        }

        node.CheckState = GetCheckState(node);
        foreach (var descendant in EnumerateLoadedDescendants(node))
        {
            descendant.CheckState = GetCheckState(descendant);
        }
    }

    public TreeViewNode FindVisibleNode(object item)
    {
        return VisibleNodes.FirstOrDefault(x => Equals(x.Item, item));
    }

    public IEnumerable<TreeViewNode> EnumerateLoadedDescendants(TreeViewNode node)
    {
        if (node is null)
        {
            yield break;
        }

        var nodeIndex = VisibleNodes.IndexOf(node);
        if (nodeIndex < 0)
        {
            yield break;
        }

        for (var i = nodeIndex + 1; i < VisibleNodes.Count && VisibleNodes[i].Depth > node.Depth; i++)
        {
            yield return VisibleNodes[i];
        }
    }

    public IEnumerable EnumerateChildItems(object item) => Enumerate(ReadChildren(item));

    private void ExpandNode(TreeViewNode node)
    {
        LoadChildrenIfNecessary(node);
        node.RefreshIsLeaf();

        if (node.IsLeaf)
        {
            SetExpanded(node, false, updateItem: true);
            return;
        }

        var nodeIndex = VisibleNodes.IndexOf(node);
        if (nodeIndex < 0 || HasVisibleChildren(nodeIndex, node.Depth))
        {
            return;
        }

        var children = CreateExpandedChildren(node);
        if (children.Count > 0)
        {
            UpdateVisibleNodes(() => VisibleNodes.InsertRange(nodeIndex + 1, children));
        }
    }

    private void CollapseNode(TreeViewNode node)
    {
        var nodeIndex = VisibleNodes.IndexOf(node);
        if (nodeIndex < 0)
        {
            return;
        }

        var count = CountVisibleDescendants(nodeIndex, node.Depth);
        if (count <= 0)
        {
            return;
        }

        IReadOnlyList<TreeViewNode> removed = Array.Empty<TreeViewNode>();
        UpdateVisibleNodes(() => removed = VisibleNodes.RemoveRange(nodeIndex + 1, count));
        DisposeNodes(removed);
    }

    private void RebuildExpandedChildren(TreeViewNode node)
    {
        var nodeIndex = VisibleNodes.IndexOf(node);
        if (nodeIndex < 0)
        {
            return;
        }

        var count = CountVisibleDescendants(nodeIndex, node.Depth);
        var children = !node.IsExpanded || node.IsLeaf
            ? Array.Empty<TreeViewNode>()
            : CreateExpandedChildren(node);

        if (count <= 0 && children.Count == 0)
        {
            return;
        }

        IReadOnlyList<TreeViewNode> removed = Array.Empty<TreeViewNode>();
        UpdateVisibleNodes(() =>
        {
            if (count > 0)
            {
                removed = VisibleNodes.RemoveRange(nodeIndex + 1, count);
            }

            if (children.Count > 0)
            {
                VisibleNodes.InsertRange(nodeIndex + 1, children);
            }
        });
        DisposeNodes(removed);
    }

    private void UpdateVisibleNodes(Action update)
    {
        VisibleNodesChanging();
        try
        {
            update();
        }
        finally
        {
            VisibleNodesChanged();
        }
    }

    private IReadOnlyList<TreeViewNode> CreateExpandedChildren(TreeViewNode parent)
    {
        var children = new List<TreeViewNode>();
        foreach (var item in Enumerate(ReadChildren(parent.Item)))
        {
            AppendNodeAndExpandedDescendants(children, CreateNode(item, parent, parent.Depth + 1));
        }

        return children;
    }

    private void AppendNodeAndExpandedDescendants(List<TreeViewNode> nodes, TreeViewNode node)
    {
        nodes.Add(node);

        if (!node.IsExpanded || node.IsLeaf)
        {
            return;
        }

        LoadChildrenIfNecessary(node);
        node.RefreshIsLeaf();

        if (node.IsLeaf)
        {
            node.SetExpandedFromController(false);
            WriteIsExpanded(node.Item, false);
            return;
        }

        foreach (var item in Enumerate(ReadChildren(node.Item)))
        {
            AppendNodeAndExpandedDescendants(nodes, CreateNode(item, node, node.Depth + 1));
        }
    }

    private TreeViewNode CreateNode(object item, TreeViewNode parent, int depth)
    {
        var node = new TreeViewNode(this, item, parent, depth);
        SubscribeChildren(node);
        return node;
    }

    private void LoadChildrenIfNecessary(TreeViewNode node)
    {
        if (LoadChildrenCommand is null || node.HasLoadedChildren || HasChildren(node.Item))
        {
            return;
        }

        if (!LoadChildrenCommand.CanExecute(node.Item))
        {
            return;
        }

        node.HasLoadedChildren = true;
        node.IsLoading = true;

        try
        {
            LoadChildrenCommand.Execute(node.Item);
        }
        finally
        {
            node.IsLoading = false;
        }
    }

    private void SubscribeRootCollection()
    {
        if (rootCollection is not null)
        {
            return;
        }

        if (itemsSource is not INotifyCollectionChanged collection)
        {
            return;
        }

        rootCollection = collection;
        rootCollectionHandler = (_, _) => Rebuild();
        rootCollection.CollectionChanged += rootCollectionHandler;
    }

    private void UnsubscribeRootCollection()
    {
        if (rootCollection is null || rootCollectionHandler is null)
        {
            return;
        }

        rootCollection.CollectionChanged -= rootCollectionHandler;
        rootCollection = null;
        rootCollectionHandler = null;
    }

    private void SubscribeChildren(TreeViewNode node)
    {
        if (childSubscriptions.ContainsKey(node))
        {
            return;
        }

        if (ReadChildren(node.Item) is not INotifyCollectionChanged collection)
        {
            return;
        }

        NotifyCollectionChangedEventHandler handler = (_, _) => OnChildrenChanged(node);
        childSubscriptions[node] = (collection, handler);
        collection.CollectionChanged += handler;
    }

    private void UnsubscribeChildren(TreeViewNode node)
    {
        if (!childSubscriptions.Remove(node, out var subscription))
        {
            return;
        }

        subscription.Collection.CollectionChanged -= subscription.Handler;
    }

    private void OnChildrenChanged(TreeViewNode node)
    {
        if (!VisibleNodes.Contains(node))
        {
            return;
        }

        node.RefreshIsLeaf();

        if (node.IsLeaf && node.IsExpanded)
        {
            SetExpanded(node, false, updateItem: true);
            return;
        }

        if (node.IsExpanded)
        {
            RebuildExpandedChildren(node);
        }
    }

    private bool HasChildren(object item)
    {
        var children = ReadChildren(item);
        if (children is null)
        {
            return false;
        }

        if (children is ICollection collection)
        {
            return collection.Count > 0;
        }

        if (children is IReadOnlyCollection<object> readOnlyObjectCollection)
        {
            return readOnlyObjectCollection.Count > 0;
        }

        var enumerator = children.GetEnumerator();
        try
        {
            return enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private IEnumerable ReadChildren(object item)
    {
        if (item is null || string.IsNullOrWhiteSpace(childrenPropertyName))
        {
            return null;
        }

        return ReadProperty(item, childrenPropertyName) as IEnumerable;
    }

    private object ReadProperty(object item, string propertyName)
    {
        if (item is null || string.IsNullOrWhiteSpace(propertyName))
        {
            return null;
        }

        var property = GetProperty(item.GetType(), propertyName);
        return property?.GetValue(item);
    }

    private bool ReadBoolean(object item, string propertyName, bool defaultValue)
    {
        var value = ReadProperty(item, propertyName);
        return value switch
        {
            bool boolValue => boolValue,
            _ => defaultValue,
        };
    }

    private void WriteIsExpanded(object item, bool value)
    {
        if (string.IsNullOrWhiteSpace(IsExpandedPropertyName) || item is null)
        {
            return;
        }

        var property = GetProperty(item.GetType(), IsExpandedPropertyName);
        if (property?.CanWrite == true && property.PropertyType == typeof(bool))
        {
            property.SetValue(item, value);
        }
    }

    private PropertyInfo GetProperty(Type type, string propertyName)
    {
        var key = (type, propertyName);
        if (propertyCache.TryGetValue(key, out var property))
        {
            return property;
        }

        property = type.GetRuntimeProperty(propertyName);
        propertyCache[key] = property;
        return property;
    }

    private static IEnumerable Enumerate(IEnumerable source)
    {
        if (source is null)
        {
            yield break;
        }

        foreach (var item in source)
        {
            yield return item;
        }
    }

    private static bool IsConfiguredProperty(string changedPropertyName, string configuredPropertyName)
    {
        return !string.IsNullOrWhiteSpace(configuredPropertyName)
            && (string.IsNullOrEmpty(changedPropertyName) || changedPropertyName == configuredPropertyName);
    }

    private bool HasVisibleChildren(int nodeIndex, int depth)
    {
        return nodeIndex + 1 < VisibleNodes.Count && VisibleNodes[nodeIndex + 1].Depth > depth;
    }

    private int CountVisibleDescendants(int nodeIndex, int depth)
    {
        var count = 0;
        for (var i = nodeIndex + 1; i < VisibleNodes.Count && VisibleNodes[i].Depth > depth; i++)
        {
            count++;
        }

        return count;
    }

    private void DisposeNodes(IEnumerable<TreeViewNode> nodes)
    {
        foreach (var node in nodes.ToArray())
        {
            UnsubscribeChildren(node);
            node.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(TreeViewDataController));
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        Detach();
    }
}
