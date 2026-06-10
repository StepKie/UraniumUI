using System.ComponentModel;
using UraniumUI.Extensions;

namespace UraniumUI.Material.Controls;

public class TreeViewHierarchicalSelectBehavior : Behavior<CheckBox>
{
    private CheckBox checkBox;
    private TreeViewNode node;

    protected override void OnAttachedTo(CheckBox bindable)
    {
        base.OnAttachedTo(bindable);
        checkBox = bindable;
        bindable.CheckChanged += CheckBox_CheckChanged;
        bindable.BindingContextChanged += CheckBox_BindingContextChanged;
        bindable.ParentChanged += CheckBox_ParentChanged;
        AttachNode(bindable);
    }

    protected override void OnDetachingFrom(CheckBox bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.CheckChanged -= CheckBox_CheckChanged;
        bindable.BindingContextChanged -= CheckBox_BindingContextChanged;
        bindable.ParentChanged -= CheckBox_ParentChanged;
        DetachNode();
        checkBox = null;
    }

    private void CheckBox_BindingContextChanged(object sender, EventArgs e)
    {
        AttachNode((CheckBox)sender);
    }

    private void CheckBox_ParentChanged(object sender, EventArgs e)
    {
        AttachNode((CheckBox)sender);
    }

    private void CheckBox_CheckChanged(object sender, EventArgs e)
    {
        var checkBox = sender as CheckBox;
        var row = FindRow(checkBox);
        if (row is null)
        {
            throw new InvalidOperationException("CheckBox isn't in a TreeView ItemTemplate");
        }

        AttachNode(checkBox);

        if (row.TreeView.IsBusy)
        {
            return;
        }

        row.TreeView.IsBusy = true;
        try
        {
            ApplyHierarchicalSelection(checkBox);
            CheckStateItself(row);
        }
        finally
        {
            row.TreeView.IsBusy = false;
        }
    }

    protected virtual void ApplyHierarchicalSelection(CheckBox checkBox)
    {
        var row = FindRow(checkBox) ?? throw new InvalidOperationException("CheckBox isn't in a TreeView ItemTemplate");
        var state = checkBox.IsChecked ? TreeViewNodeCheckState.Checked : TreeViewNodeCheckState.Unchecked;
        row.TreeView.ApplyHierarchicalCheckState(row.Node, state);
    }

    protected virtual void CheckStateItself(TreeViewNodeView row, bool forcedSemiSelected = false)
    {
        if (row?.Node is not null)
        {
            ApplyCheckState(FindCheckBox(row), row.Node.CheckState);
        }
    }

    private void AttachNode(CheckBox checkBox)
    {
        var row = FindRow(checkBox);
        if (row?.Node is null || ReferenceEquals(node, row.Node))
        {
            if (node is not null)
            {
                ApplyCheckState(checkBox, node.CheckState);
            }

            return;
        }

        DetachNode();
        node = row.Node;
        node.PropertyChanged += Node_PropertyChanged;
        ApplyCheckState(checkBox, node.CheckState);
    }

    private void DetachNode()
    {
        if (node is not null)
        {
            node.PropertyChanged -= Node_PropertyChanged;
            node = null;
        }
    }

    private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TreeViewNode.CheckState) && checkBox is not null && sender is TreeViewNode treeNode)
        {
            ApplyCheckState(checkBox, treeNode.CheckState);
        }
    }

    private static void ApplyCheckState(CheckBox checkBox, TreeViewNodeCheckState state)
    {
        var row = FindRow(checkBox);
        if (row is null)
        {
            return;
        }

        var wasBusy = row.TreeView.IsBusy;
        row.TreeView.IsBusy = true;
        try
        {
            checkBox.IsChecked = state != TreeViewNodeCheckState.Unchecked;
            checkBox.IconGeometry = state == TreeViewNodeCheckState.Indeterminate
                ? InputKit.Shared.Controls.PredefinedShapes.Line
                : InputKit.Shared.Controls.PredefinedShapes.Check;
        }
        finally
        {
            row.TreeView.IsBusy = wasBusy;
        }
    }

    private static TreeViewNodeView FindRow(CheckBox checkBox)
    {
        return checkBox?.FindInParents<TreeViewNodeView>();
    }

    private static CheckBox FindCheckBox(TreeViewNodeView row)
    {
        return row?.FindInChildrenHierarchy<CheckBox>();
    }
}
