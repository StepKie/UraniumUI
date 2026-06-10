using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace UraniumUI.Material.Controls;

internal sealed class TreeViewVisibleNodeCollection : ObservableCollection<TreeViewNode>
{
    public void ReplaceWith(IEnumerable<TreeViewNode> nodes)
    {
        CheckReentrancy();

        Items.Clear();
        foreach (var node in nodes)
        {
            Items.Add(node);
        }

        RaiseReset();
    }

    public void InsertRange(int index, IReadOnlyList<TreeViewNode> nodes)
    {
        if (nodes.Count == 0)
        {
            return;
        }

        CheckReentrancy();

        for (var i = 0; i < nodes.Count; i++)
        {
            Items.Insert(index + i, nodes[i]);
        }

        RaiseRangeChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)nodes, index));
    }

    public IReadOnlyList<TreeViewNode> RemoveRange(int index, int count)
    {
        if (count <= 0)
        {
            return Array.Empty<TreeViewNode>();
        }

        CheckReentrancy();

        var removed = new List<TreeViewNode>(count);
        for (var i = 0; i < count; i++)
        {
            removed.Add(Items[index]);
            Items.RemoveAt(index);
        }

        RaiseRangeChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
        return removed;
    }

    private void RaiseRangeChanged(NotifyCollectionChangedEventArgs args)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(args);
    }

    private void RaiseReset()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
