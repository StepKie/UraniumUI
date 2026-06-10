using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Shouldly;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;

namespace UraniumUI.Material.Tests.Controls;

public class TreeView_Test
{
    public TreeView_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void ItemsSource_ShouldBeSet_FromViewModel()
    {
        var itemSource = new[] { "1", "2", "3", "4" };
        var viewModel = new TestViewModel { ItemSource = itemSource };

        var control = AnimationReadyHandler.Prepare(new TreeView());
        control.BindingContext = viewModel;

        control.SetBinding(TreeView.ItemsSourceProperty, new Binding(nameof(TestViewModel.ItemSource)));

        control.ItemsSource.ShouldBe(itemSource);
    }

    [Fact]
    public void SelectedItem_ShouldBeSet_FromViewModel()
    {
        var itemSource = new[] { "1", "2", "3", "4" };
        var viewModel = new TestViewModel { ItemSource = itemSource };

        var control = AnimationReadyHandler.Prepare(new TreeView());
        control.BindingContext = viewModel;
        control.SetBinding(TreeView.ItemsSourceProperty, new Binding(nameof(TestViewModel.ItemSource)));
        control.SetBinding(TreeView.SelectedItemProperty, new Binding(nameof(TestViewModel.SelectedItem)));

        viewModel.SelectedItem = itemSource[0];

        control.SelectedItem.ShouldBe(itemSource[0]);
    }

    [Fact]
    public void SelectedItem_ShouldBeSet_FromControl()
    {
        var itemSource = new[] { "1", "2", "3", "4" };
        var viewModel = new TestViewModel { ItemSource = itemSource };

        var control = AnimationReadyHandler.Prepare(new TreeView());
        control.BindingContext = viewModel;
        control.SetBinding(TreeView.ItemsSourceProperty, new Binding(nameof(TestViewModel.ItemSource)));
        control.SetBinding(TreeView.SelectedItemProperty, new Binding(nameof(TestViewModel.SelectedItem)));

        control.SelectedItem = itemSource[0];

        viewModel.SelectedItem.ShouldBe(itemSource[0]);
    }

    [Fact]
    public void DataController_ShouldShowOnlyRootNodes_Initially()
    {
        var roots = new[]
        {
            Node("A", Node("A.1")),
            Node("B", Node("B.1")),
        };
        var controller = CreateController();

        controller.SetItemsSource(roots);

        Names(controller).ShouldBe(["A", "B"]);
        Depths(controller).ShouldBe([0, 0]);
    }

    [Fact]
    public void ExpandingNode_ShouldInsertChildren_AfterParent()
    {
        var root = Node("A", Node("A.1"), Node("A.2"));
        var controller = CreateController();
        controller.SetItemsSource(new[] { root, Node("B") });

        controller.VisibleNodes[0].IsExpanded = true;

        Names(controller).ShouldBe(["A", "A.1", "A.2", "B"]);
        Depths(controller).ShouldBe([0, 1, 1, 0]);
    }

    [Fact]
    public void CollapsingNode_ShouldRemoveAllVisibleDescendants_Only()
    {
        var root = Node("A", Node("A.1", Node("A.1.a")), Node("A.2"));
        root.IsExpanded = true;
        root.Children[0].IsExpanded = true;
        var controller = CreateController(isExpandedPropertyName: nameof(TestNode.IsExpanded));
        controller.SetItemsSource(new[] { root, Node("B") });

        controller.VisibleNodes[0].IsExpanded = false;

        Names(controller).ShouldBe(["A", "B"]);
    }

    [Fact]
    public void InitialBuild_ShouldRespectExpandedProperty_Recursively()
    {
        var root = Node("A", Node("A.1", Node("A.1.a")), Node("A.2"));
        root.IsExpanded = true;
        root.Children[0].IsExpanded = true;
        var controller = CreateController(isExpandedPropertyName: nameof(TestNode.IsExpanded));

        controller.SetItemsSource(new[] { root });

        Names(controller).ShouldBe(["A", "A.1", "A.1.a", "A.2"]);
        Depths(controller).ShouldBe([0, 1, 2, 1]);
    }

    [Fact]
    public void ExpandingNode_ShouldWriteExpandedProperty_ToItem()
    {
        var root = Node("A", Node("A.1"));
        var controller = CreateController(isExpandedPropertyName: nameof(TestNode.IsExpanded));
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsExpanded = true;

        root.IsExpanded.ShouldBeTrue();
    }

    [Fact]
    public void ExternalExpandedPropertyChange_ShouldUpdateVisibleNodes()
    {
        var root = Node("A", Node("A.1"));
        var controller = CreateController(isExpandedPropertyName: nameof(TestNode.IsExpanded));
        controller.SetItemsSource(new[] { root });

        root.IsExpanded = true;

        Names(controller).ShouldBe(["A", "A.1"]);
    }

    [Fact]
    public void LeafProperty_ShouldPreventExpansion()
    {
        var root = Node("A", Node("A.1"));
        root.IsLeaf = true;
        var controller = CreateController(isLeafPropertyName: nameof(TestNode.IsLeaf));
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsExpanded = true;

        Names(controller).ShouldBe(["A"]);
        controller.VisibleNodes[0].IsExpanded.ShouldBeFalse();
    }

    [Fact]
    public void ChildrenBinding_ShouldReadCustomChildProperty()
    {
        var root = Node("A");
        root.SubItems.Add(Node("A.sub"));
        var controller = CreateController(childrenBinding: new Binding(nameof(TestNode.SubItems)));
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsExpanded = true;

        Names(controller).ShouldBe(["A", "A.sub"]);
    }

    [Fact]
    public void LazyLoadCommand_ShouldExecuteOnce_ForExpandedUnloadedNode()
    {
        var root = Node("A");
        root.IsLeaf = false;
        var executeCount = 0;
        var controller = CreateController(isLeafPropertyName: nameof(TestNode.IsLeaf));
        controller.LoadChildrenCommand = new TestCommand<TestNode>(node =>
        {
            executeCount++;
            node.Children.Add(Node("A.lazy"));
        });
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsExpanded = true;
        controller.VisibleNodes[0].IsExpanded = false;
        controller.VisibleNodes[0].IsExpanded = true;

        executeCount.ShouldBe(1);
        Names(controller).ShouldBe(["A", "A.lazy"]);
    }

    [Fact]
    public void LazyLoadCommand_ShouldAllowChildren_ToArriveAfterExpansion()
    {
        var root = Node("A");
        root.IsLeaf = false;
        var controller = CreateController(isLeafPropertyName: nameof(TestNode.IsLeaf));
        controller.LoadChildrenCommand = new TestCommand<TestNode>(_ => { });
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsExpanded = true;
        root.Children.Add(Node("A.async"));

        Names(controller).ShouldBe(["A", "A.async"]);
    }

    [Fact]
    public void RootCollectionChanges_ShouldRebuildVisibleNodes()
    {
        var roots = new TrackingCollection<TestNode> { Node("A") };
        var controller = CreateController();
        controller.SetItemsSource(roots);

        roots.Add(Node("B"));

        Names(controller).ShouldBe(["A", "B"]);
    }

    [Fact]
    public void CollapsedChildCollectionAdd_ShouldRefreshLeafState_WithoutShowingChild()
    {
        var root = Node("A");
        var controller = CreateController();
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsLeaf.ShouldBeTrue();
        root.Children.Add(Node("A.1"));

        controller.VisibleNodes[0].IsLeaf.ShouldBeFalse();
        Names(controller).ShouldBe(["A"]);
    }

    [Fact]
    public void ExpandedChildCollectionAdd_ShouldInsertNewChild()
    {
        var root = Node("A", Node("A.1"));
        var controller = CreateController();
        controller.SetItemsSource(new[] { root });
        controller.VisibleNodes[0].IsExpanded = true;

        root.Children.Add(Node("A.2"));

        Names(controller).ShouldBe(["A", "A.1", "A.2"]);
    }

    [Fact]
    public void ExpandedChildCollectionRemove_ShouldRemoveChildAndDescendants()
    {
        var child = Node("A.1", Node("A.1.a"));
        child.IsExpanded = true;
        var root = Node("A", child, Node("A.2"));
        root.IsExpanded = true;
        var controller = CreateController(isExpandedPropertyName: nameof(TestNode.IsExpanded));
        controller.SetItemsSource(new[] { root });

        root.Children.Remove(child);

        Names(controller).ShouldBe(["A", "A.2"]);
    }

    [Fact]
    public void ChildrenCollectionReplacement_ShouldUnsubscribeOldCollection_AndUseNewCollection()
    {
        var oldChildren = new TrackingCollection<TestNode> { Node("old") };
        var newChildren = new TrackingCollection<TestNode> { Node("new") };
        var root = Node("A");
        root.Children = oldChildren;
        var controller = CreateController();
        controller.SetItemsSource(new[] { root });
        controller.VisibleNodes[0].IsExpanded = true;

        root.Children = newChildren;

        oldChildren.SubscriberCount.ShouldBe(0);
        Names(controller).ShouldBe(["A", "new"]);
    }

    [Fact]
    public void SingleSelectionRefresh_ShouldUpdateMatchingVisibleNode()
    {
        var root = Node("A", Node("A.1"));
        object selected = null;
        var controller = CreateController();
        controller.IsItemSelected = item => Equals(item, selected);
        controller.SetItemsSource(new[] { root });
        controller.VisibleNodes[0].IsExpanded = true;

        selected = root.Children[0];
        controller.RefreshSelectionForItem(selected);

        controller.VisibleNodes[1].IsSelected.ShouldBeTrue();
        controller.VisibleNodes[0].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void NewVisibleNodes_ShouldUseCurrentSelectionState()
    {
        var root = Node("A", Node("A.1"));
        var selectedItems = new HashSet<object> { root.Children[0] };
        var controller = CreateController();
        controller.IsItemSelected = selectedItems.Contains;
        controller.SetItemsSource(new[] { root });

        controller.VisibleNodes[0].IsExpanded = true;

        controller.VisibleNodes[1].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void Detach_ShouldUnsubscribeRootAndChildCollections()
    {
        var root = Node("A", Node("A.1"));
        var roots = new TrackingCollection<TestNode> { root };
        var controller = CreateController();
        controller.SetItemsSource(roots);
        controller.VisibleNodes[0].IsExpanded = true;

        roots.SubscriberCount.ShouldBe(1);
        root.Children.SubscriberCount.ShouldBe(1);

        controller.Detach();

        roots.SubscriberCount.ShouldBe(0);
        root.Children.SubscriberCount.ShouldBe(0);
        controller.VisibleNodes.Count.ShouldBe(0);
    }

    [Fact]
    public void DetachedController_ShouldNotSubscribeNewItemsSource_UntilAttached()
    {
        var controller = CreateController();
        controller.SetItemsSource(new TrackingCollection<TestNode> { Node("old") });
        controller.Detach();
        var newRoots = new TrackingCollection<TestNode> { Node("new") };

        controller.SetItemsSource(newRoots);

        newRoots.SubscriberCount.ShouldBe(0);
        controller.VisibleNodes.Count.ShouldBe(0);

        controller.Attach();

        newRoots.SubscriberCount.ShouldBe(1);
        Names(controller).ShouldBe(["new"]);
    }

    [Fact]
    public void ExpandingLargeBranch_ShouldBatchVisibleCollectionNotification()
    {
        var root = Node("root");
        for (var i = 0; i < 10_000; i++)
        {
            root.Children.Add(Node($"child-{i}"));
        }

        var controller = CreateController();
        controller.SetItemsSource(new[] { root });
        var notificationCount = 0;
        controller.VisibleNodes.CollectionChanged += (_, _) => notificationCount++;

        controller.VisibleNodes[0].IsExpanded = true;

        controller.VisibleNodes.Count.ShouldBe(10_001);
        notificationCount.ShouldBe(1);
    }

    [Fact]
    public void TreeView_ShouldExposeFlatVisibleNodes_WhenExpanded()
    {
        var root = Node("A", Node("A.1"));
        var control = AnimationReadyHandler.Prepare(new TreeView
        {
            ItemsSource = new[] { root }
        });

        control.VisibleNodes[0].IsExpanded = true;

        control.VisibleNodes.Select(x => ((TestNode)x.Item).Name).ToArray().ShouldBe(["A", "A.1"]);
        control.Content.ShouldBeOfType<CollectionView>();
    }

    [Fact]
    public void TreeViewItemTemplate_ShouldReceiveOriginalItem_AsBindingContext()
    {
        var root = Node("A");
        var control = AnimationReadyHandler.Prepare(new TreeView
        {
            ItemsSource = new[] { root },
            ItemTemplate = new DataTemplate(() => new Label())
        });
        var collectionView = control.Content.ShouldBeOfType<CollectionView>();
        var row = collectionView.ItemTemplate.CreateContent().ShouldBeOfType<TreeViewNodeView>();

        row.BindingContext = control.VisibleNodes[0];

        var rowButton = row.Content.ShouldBeOfType<UraniumUI.Views.StatefulContentView>();
        var grid = rowButton.Content.ShouldBeOfType<Grid>();
        var itemHost = grid.Children.OfType<TreeViewNodeItemContentView>().Single();
        itemHost.Content.ShouldBeOfType<Label>().BindingContext.ShouldBe(root);
    }

    private static TreeViewDataController CreateController(
        BindingBase childrenBinding = null,
        string isExpandedPropertyName = null,
        string isLeafPropertyName = null)
    {
        var controller = new TreeViewDataController
        {
            IsExpandedPropertyName = isExpandedPropertyName,
            IsLeafPropertyName = isLeafPropertyName,
        };

        if (childrenBinding is not null)
        {
            controller.SetChildrenBinding(childrenBinding);
        }

        return controller;
    }

    private static TestNode Node(string name, params TestNode[] children)
    {
        var node = new TestNode { Name = name };
        foreach (var child in children)
        {
            node.Children.Add(child);
        }

        return node;
    }

    private static string[] Names(TreeViewDataController controller)
    {
        return controller.VisibleNodes.Select(x => ((TestNode)x.Item).Name).ToArray();
    }

    private static int[] Depths(TreeViewDataController controller)
    {
        return controller.VisibleNodes.Select(x => x.Depth).ToArray();
    }

    public class TestViewModel : UraniumBindableObject
    {
        private IList itemSource;
        private object selectedItem;

        public IList ItemSource { get => itemSource; set => SetProperty(ref itemSource, value); }

        public object SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }
    }

    private sealed class TestNode : UraniumBindableObject
    {
        private TrackingCollection<TestNode> children = new();
        private TrackingCollection<TestNode> subItems = new();
        private bool isExpanded;
        private bool isLeaf;

        public string Name { get; set; }

        public TrackingCollection<TestNode> Children { get => children; set => SetProperty(ref children, value); }

        public TrackingCollection<TestNode> SubItems { get => subItems; set => SetProperty(ref subItems, value); }

        public bool IsExpanded { get => isExpanded; set => SetProperty(ref isExpanded, value); }

        public bool IsLeaf { get => isLeaf; set => SetProperty(ref isLeaf, value); }
    }

    private sealed class TrackingCollection<T> : ObservableCollection<T>
    {
        private int subscriberCount;

        public int SubscriberCount => subscriberCount;

        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                subscriberCount++;
                base.CollectionChanged += value;
            }
            remove
            {
                subscriberCount--;
                base.CollectionChanged -= value;
            }
        }
    }

    private sealed class TestCommand<T> : ICommand
    {
        private readonly Action<T> execute;

        public TestCommand(Action<T> execute)
        {
            this.execute = execute;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => execute((T)parameter);
    }
}
