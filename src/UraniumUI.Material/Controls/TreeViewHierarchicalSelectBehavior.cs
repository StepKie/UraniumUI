using UraniumUI.Extensions;

namespace UraniumUI.Material.Controls;

public class TreeViewHierarchicalSelectBehavior : Behavior<CheckBox>
{
    protected override void OnAttachedTo(CheckBox bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.CheckChanged += CheckBox_CheckChanged;
    }

    protected override void OnDetachingFrom(CheckBox bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.CheckChanged -= CheckBox_CheckChanged;
    }

    private void CheckBox_CheckChanged(object sender, EventArgs e)
    {
        var checkBox = sender as CheckBox;
        var row = checkBox.FindInParents<TreeViewNodeView>();
        if (row is null)
        {
            throw new InvalidOperationException("CheckBox isn't in a TreeView ItemTemplate");
        }

        lock (row.TreeView)
        {
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
    }

    protected virtual void ApplyHierarchicalSelection(CheckBox checkBox)
    {
        var row = checkBox.FindInParents<TreeViewNodeView>() ?? throw new InvalidOperationException("CheckBox isn't in a TreeView ItemTemplate");

        if (!row.Node.IsExpanded && !row.Node.IsLeaf)
        {
            row.Node.IsExpanded = true;
        }

        foreach (var descendant in row.TreeView.GetLoadedDescendants(row.Node).ToList())
        {
            var descendantRow = FindRow(row.TreeView, descendant);
            var childCheckBox = descendantRow is null ? null : FindCheckBox(descendantRow);
            if (childCheckBox is not null && childCheckBox.IsChecked != checkBox.IsChecked)
            {
                childCheckBox.IsChecked = checkBox.IsChecked;
            }
        }
    }

    protected virtual void CheckStateItself(TreeViewNodeView row, bool forcedSemiSelected = false)
    {
        if (row?.Node?.Parent is null)
        {
            return;
        }

        var parentRow = FindRow(row.TreeView, row.Node.Parent);
        if (parentRow is null)
        {
            return;
        }

        var parentCheckBox = FindCheckBox(parentRow);
        if (parentCheckBox is null)
        {
            return;
        }

        if (forcedSemiSelected)
        {
            parentCheckBox.IconGeometry = InputKit.Shared.Controls.PredefinedShapes.Line;
            if (!parentCheckBox.IsChecked)
            {
                parentCheckBox.IsChecked = true;
            }
            return;
        }

        parentCheckBox.IconGeometry = InputKit.Shared.Controls.PredefinedShapes.Check;

        var childRows = row.TreeView.GetLoadedDescendants(parentRow.Node)
            .Where(x => x.Parent == parentRow.Node)
            .Select(x => FindRow(row.TreeView, x))
            .Where(x => x is not null)
            .ToList();

        if (childRows.Count > 0)
        {
            var firstCheck = FindCheckBox(childRows[0])?.IsChecked ?? false;

            foreach (var childRow in childRows)
            {
                var childCheckBox = FindCheckBox(childRow);
                if (childCheckBox is not null && childCheckBox.IsChecked != firstCheck)
                {
                    parentCheckBox.IconGeometry = InputKit.Shared.Controls.PredefinedShapes.Line;
                    if (!parentCheckBox.IsChecked)
                    {
                        parentCheckBox.IsChecked = true;
                    }

                    CheckStateItself(parentRow, true);
                    return;
                }
            }

            if (parentCheckBox.IsChecked != firstCheck)
            {
                parentCheckBox.IsChecked = firstCheck;
            }
        }

        CheckStateItself(parentRow);
    }

    private static TreeViewNodeView FindRow(TreeView treeView, TreeViewNode node)
    {
        return treeView.FindManyInChildrenHierarchy<TreeViewNodeView>()
            .FirstOrDefault(x => ReferenceEquals(x.Node, node));
    }

    private static CheckBox FindCheckBox(TreeViewNodeView row)
    {
        if (row is null)
        {
            return null;
        }

        return row.FindInChildrenHierarchy<CheckBox>();
    }
}
