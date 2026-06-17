using System.ComponentModel;
using UraniumUI.Extensions;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.Views;
using static Microsoft.Maui.Controls.VisualStateManager;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Controls;

public sealed class TreeViewNodeView : ContentView
{
    private readonly TreeView treeView;
    private readonly TreeViewNodeItemContentView nodeContainer;
    private readonly StatefulContentView rowButton;
    private readonly Grid rowGrid;
    private readonly View expanderView;
    private readonly ButtonView defaultExpanderButton;
    private TreeViewNode node;

    public TreeViewNodeView(TreeView treeView)
    {
        this.treeView = treeView;
        nodeContainer = new TreeViewNodeItemContentView
        {
            ItemTemplate = treeView.ItemTemplate,
            VerticalOptions = LayoutOptions.Center
        };

        expanderView = CreateExpanderView(out var expanderButton);
        defaultExpanderButton = expanderButton;

        rowGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(40),
                new ColumnDefinition(GridLength.Star),
            }
        };
        rowGrid.Add(expanderView);
        rowGrid.Add(nodeContainer, column: 1);

        rowButton = new StatefulContentView
        {
            Content = rowGrid,
            TappedCommand = new Command(OnRowTapped),
        };

        Content = rowButton;
    }

    public TreeView TreeView => treeView;

    public TreeViewNode Node => node;

    protected override void OnBindingContextChanged()
    {
        if (node is not null)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
        }

        base.OnBindingContextChanged();

        node = BindingContext as TreeViewNode;
        if (node is null)
        {
            nodeContainer.Item = null;
            expanderView.BindingContext = null;
            return;
        }

        node.PropertyChanged += OnNodePropertyChanged;
        nodeContainer.ItemTemplate = treeView.ItemTemplate;
        nodeContainer.Item = node.Item;
        expanderView.BindingContext = node;
        rowButton.Padding = new Thickness(node.Depth * 16, 0, 0, 0);

        UpdateExpanderState(animate: false);
        UpdateSelectionState();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (defaultExpanderButton is not null)
        {
            defaultExpanderButton.RotationY = this.IsRtl() ? 180 : 0;
        }
    }

    private View CreateExpanderView(out ButtonView defaultButton)
    {
        defaultButton = null;

        if (treeView.ExpanderTemplate?.CreateContent() is View customExpander)
        {
            return customExpander;
        }

        defaultButton = new ButtonView
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            StyleClass = new[] { "TreeViewExpandButton" },
            Padding = 0,
            Margin = 0,
            TappedCommand = new Command(ToggleExpanded),
        };

        defaultButton.Content = new ContentView
        {
            Margin = new Thickness(0, 0, 5, 0),
            Content = new Path
            {
                StyleClass = new[] { "TreeView.Arrow" },
                Data = UraniumShapes.ArrowRight,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }
        };

        return defaultButton;
    }

    private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TreeViewNode.IsExpanded) || e.PropertyName == nameof(TreeViewNode.IsLeaf))
        {
            UpdateExpanderState(animate: e.PropertyName == nameof(TreeViewNode.IsExpanded));
        }

        if (e.PropertyName == nameof(TreeViewNode.IsSelected))
        {
            UpdateSelectionState();
        }
    }

    private void ToggleExpanded()
    {
        if (node is null || node.IsLeaf)
        {
            return;
        }

        node.IsExpanded = !node.IsExpanded;
    }

    private void OnRowTapped()
    {
        if (node is not null)
        {
            treeView.OnNodeClicked(node);
        }
    }

    private void UpdateExpanderState(bool animate)
    {
        if (node is null || defaultExpanderButton is null)
        {
            return;
        }

        defaultExpanderButton.Opacity = node.IsLeaf ? 0 : 1;
        defaultExpanderButton.InputTransparent = node.IsLeaf;
        defaultExpanderButton.IsEnabled = !node.IsLeaf;
        UpdateExpanderSemantics();

        var rotation = node.IsExpanded ? 90 : 0;
        if (animate && treeView.UseAnimation)
        {
            defaultExpanderButton.RotateToSafely(rotation, 90, easing: Easing.BounceOut).FireAndForget();
        }
        else
        {
            defaultExpanderButton.Rotation = rotation;
        }
    }

    private void UpdateSelectionState()
    {
        UpdateRowSemantics();

        if (node?.IsSelected == true)
        {
            VisualStateManager.GoToState(rowButton, CommonStates.Selected);
            if (treeView.SelectionBrush is not null)
            {
                rowButton.Background = treeView.SelectionBrush;
            }
            else
            {
                rowButton.BackgroundColor = treeView.SelectionColor;
            }

            foreach (var path in rowButton.FindManyInChildrenHierarchy<Path>())
            {
                path.StyleClass = new[] { "TreeView.Arrow.Selected" };
            }

            foreach (var label in rowButton.FindManyInChildrenHierarchy<Label>())
            {
                label.StyleClass = new[] { "TreeView.Label.Selected" };
            }
        }
        else
        {
            VisualStateManager.GoToState(rowButton, CommonStates.Normal);
            rowButton.BackgroundColor = Colors.Transparent;
            rowButton.Background = Brush.Default;

            foreach (var path in rowButton.FindManyInChildrenHierarchy<Path>())
            {
                path.StyleClass = new[] { "TreeView.Arrow" };
            }

            foreach (var label in rowButton.FindManyInChildrenHierarchy<Label>())
            {
                label.StyleClass = new[] { "TreeView.Label" };
            }
        }
    }

    private void UpdateExpanderSemantics()
    {
        if (node is null || defaultExpanderButton is null)
        {
            return;
        }

        var options = AccessibilityOptionsProvider.Get();
        var nodeText = GetNodeSemanticText();
        var description = node.IsExpanded
            ? options.FormatCollapseTreeNodeDescription(nodeText)
            : options.FormatExpandTreeNodeDescription(nodeText);

        SemanticProperties.SetDescription(defaultExpanderButton, description);
    }

    private void UpdateRowSemantics()
    {
        if (node is null)
        {
            return;
        }

        var options = AccessibilityOptionsProvider.Get();
        var nodeText = GetNodeSemanticText();
        var description = node.IsSelected
            ? options.FormatSelectedTreeNodeDescription(nodeText)
            : options.FormatTreeNodeDescription(nodeText);

        SemanticProperties.SetDescription(rowButton, description);
        SemanticProperties.SetHint(rowButton, options.TreeNodeHint);
    }

    private string GetNodeSemanticText()
    {
        return node?.Item?.ToString() ?? nameof(TreeViewNode);
    }
}
