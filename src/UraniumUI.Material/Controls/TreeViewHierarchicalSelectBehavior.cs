using System.ComponentModel;
using UraniumUI.Extensions;

namespace UraniumUI.Material.Controls;

public class TreeViewHierarchicalSelectBehavior : Behavior<CheckBox>
{
    private CheckBox checkBox;
    private TreeViewNode node;
    private int suppressCheckChanged;

    protected override void OnAttachedTo(CheckBox bindable)
    {
        base.OnAttachedTo(bindable);
        checkBox = bindable;
        bindable.CheckChanged += CheckBox_CheckChanged;
        bindable.BindingContextChanged += CheckBox_BindingContextChanged;
        bindable.ParentChanged += CheckBox_ParentChanged;
        AttachNode(bindable, syncCheckBox: true);
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
        AttachNode((CheckBox)sender, syncCheckBox: true);
    }

    private void CheckBox_ParentChanged(object sender, EventArgs e)
    {
        AttachNode((CheckBox)sender, syncCheckBox: true);
    }

    private void CheckBox_CheckChanged(object sender, EventArgs e)
    {
        if (suppressCheckChanged > 0)
        {
            return;
        }

        var checkBox = sender as CheckBox;
        var row = FindRow(checkBox);
        if (row is null)
        {
            throw new InvalidOperationException("CheckBox isn't in a TreeView ItemTemplate");
        }

        AttachNode(checkBox, syncCheckBox: false);
        ApplyHierarchicalSelection(checkBox);
        CheckStateItself(row);
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

    private void AttachNode(CheckBox checkBox, bool syncCheckBox)
    {
        var row = FindRow(checkBox);
        if (row?.Node is null || ReferenceEquals(node, row.Node))
        {
            if (syncCheckBox && node is not null)
            {
                ApplyCheckState(checkBox, node.CheckState);
            }

            return;
        }

        DetachNode();
        node = row.Node;
        node.PropertyChanged += Node_PropertyChanged;
        if (syncCheckBox)
        {
            ApplyCheckState(checkBox, node.CheckState);
        }
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

    private void ApplyCheckState(CheckBox checkBox, TreeViewNodeCheckState state)
    {
        if (checkBox is null)
        {
            return;
        }

        suppressCheckChanged++;
        try
        {
            checkBox.IconGeometry = state == TreeViewNodeCheckState.Indeterminate
                ? InputKit.Shared.Controls.PredefinedShapes.Line
                : InputKit.Shared.Controls.PredefinedShapes.Check;

            var isChecked = state != TreeViewNodeCheckState.Unchecked;
            if (checkBox.IsChecked != isChecked)
            {
                checkBox.IsChecked = isChecked;
            }
        }
        finally
        {
            suppressCheckChanged--;
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
