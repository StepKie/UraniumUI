using System.ComponentModel;
using UraniumUI.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Tests.Controls;

public class MauiDropdown_Test
{
    public MauiDropdown_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void ItemTemplate_ShouldCreateView_WithItemBindingContext()
    {
        var control = new TestMauiDropdown
        {
            ItemTemplate = new DataTemplate(() => new Label())
        };
        var item = new TestItem("Ada");

        var view = control.CreateItem(item);

        Assert.IsType<Label>(view);
        Assert.Same(item, view.BindingContext);
    }

    [Fact]
    public void ItemDisplayBinding_ShouldSetDefaultItemText()
    {
        var control = new TestMauiDropdown
        {
            ItemDisplayBinding = new Binding(nameof(TestItem.Name))
        };

        var view = Assert.IsType<Label>(control.CreateItem(new TestItem("Grace")));

        Assert.Equal("Grace", view.Text);
    }

    [Fact]
    public void SelectedItem_ShouldUpdateClosedStateText()
    {
        var control = new MauiDropdown
        {
            Placeholder = "Choose",
            ItemDisplayBinding = new Binding(nameof(TestItem.Name))
        };
        var selectedContent = GetSelectedContent(control);
        var selectedLabel = Assert.IsType<Label>(selectedContent.Content);

        Assert.Equal("Choose", selectedLabel.Text);

        control.SelectedItem = new TestItem("Katherine");

        selectedLabel = Assert.IsType<Label>(selectedContent.Content);
        Assert.Equal("Katherine", selectedLabel.Text);
    }

    [Fact]
    public void SelectedItem_ShouldUseItemTemplate_WhenSelectedItemTemplateIsNotSet()
    {
        var item = new TestItem("Ada");
        var control = new MauiDropdown
        {
            ItemTemplate = new DataTemplate(() => new Label { StyleId = "ItemTemplate" }),
            SelectedItem = item,
        };

        var selectedContent = GetSelectedContent(control);
        var selectedView = Assert.IsType<Label>(selectedContent.Content);

        Assert.Equal("ItemTemplate", selectedView.StyleId);
        Assert.Same(item, selectedView.BindingContext);
    }

    [Fact]
    public void SelectedItem_ShouldPreferSelectedItemTemplate()
    {
        var item = new TestItem("Ada");
        var control = new MauiDropdown
        {
            ItemTemplate = new DataTemplate(() => new Label { StyleId = "ItemTemplate" }),
            SelectedItemTemplate = new DataTemplate(() => new Label { StyleId = "SelectedItemTemplate" }),
            SelectedItem = item,
        };

        var selectedContent = GetSelectedContent(control);
        var selectedView = Assert.IsType<Label>(selectedContent.Content);

        Assert.Equal("SelectedItemTemplate", selectedView.StyleId);
        Assert.Same(item, selectedView.BindingContext);
    }

    [Fact]
    public void SelectedItemChangedProgrammatically_ShouldRefreshSelectedTemplateView()
    {
        var firstItem = new TestItem("Ada");
        var secondItem = new TestItem("Grace");
        var control = new MauiDropdown
        {
            ItemTemplate = new DataTemplate(() => new Label { StyleId = "ItemTemplate" }),
            SelectedItem = firstItem,
        };
        var selectedContent = GetSelectedContent(control);
        var firstSelectedView = Assert.IsType<Label>(selectedContent.Content);

        control.SelectedItem = secondItem;

        var secondSelectedView = Assert.IsType<Label>(selectedContent.Content);
        Assert.NotSame(firstSelectedView, secondSelectedView);
        Assert.Same(secondItem, secondSelectedView.BindingContext);
    }

    [Fact]
    public void SelectedItemPropertyChanged_ShouldRefreshSelectedText()
    {
        var item = new MutableTestItem("Ada");
        var control = new MauiDropdown
        {
            ItemDisplayBinding = new Binding(nameof(MutableTestItem.Name)),
            SelectedItem = item,
        };
        var selectedContent = GetSelectedContent(control);
        var selectedLabel = Assert.IsType<Label>(selectedContent.Content);

        Assert.Equal("Ada", selectedLabel.Text);

        item.Name = "Grace";

        selectedLabel = Assert.IsType<Label>(selectedContent.Content);
        Assert.Equal("Grace", selectedLabel.Text);
    }

    [Fact]
    public void OldSelectedItemPropertyChanged_ShouldNotRefreshSelectedTemplateView()
    {
        var oldItem = new MutableTestItem("Ada");
        var currentItem = new MutableTestItem("Grace");
        var control = new MauiDropdown
        {
            ItemTemplate = new DataTemplate(() => new Label()),
            SelectedItem = oldItem,
        };
        var selectedContent = GetSelectedContent(control);

        control.SelectedItem = currentItem;
        var currentSelectedView = selectedContent.Content;

        oldItem.Name = "Katherine";

        Assert.Same(currentSelectedView, selectedContent.Content);

        currentItem.Name = "Margaret";

        Assert.NotSame(currentSelectedView, selectedContent.Content);
    }

    [Fact]
    public void ItemTemplateChanged_ShouldRefreshSelectedItemView()
    {
        var item = new TestItem("Ada");
        var control = new MauiDropdown
        {
            SelectedItem = item,
        };

        control.ItemTemplate = new DataTemplate(() => new Label { StyleId = "ItemTemplate" });

        var selectedContent = GetSelectedContent(control);
        var selectedView = Assert.IsType<Label>(selectedContent.Content);

        Assert.Equal("ItemTemplate", selectedView.StyleId);
        Assert.Same(item, selectedView.BindingContext);
    }

    [Fact]
    public void Constructor_ShouldUsePathForArrow()
    {
        var control = new MauiDropdown();

        var arrowPath = GetArrowPath(control);

        Assert.NotNull(arrowPath.Data);
    }

    [Fact]
    public void OpenAndClose_ShouldRotateArrow()
    {
        var pageContent = new VerticalStackLayout();
        var control = new MauiDropdown
        {
            ItemsSource = new[] { "One", "Two" }
        };
        _ = new ContentPage
        {
            Content = pageContent
        };
        pageContent.Add(control);
        var arrowPath = GetArrowPath(control);

        control.Open();

        Assert.Equal(180d, arrowPath.Rotation);

        control.Close();

        Assert.Equal(0d, arrowPath.Rotation);
    }

    [Fact]
    public void Close_ShouldRestoreWrappedNonGridContent_ForPageInteraction()
    {
        var pageContent = new VerticalStackLayout();
        var page = new ContentPage
        {
            Content = pageContent
        };
        var control = new MauiDropdown
        {
            ItemsSource = new[] { "One", "Two" }
        };
        pageContent.Add(control);

        control.Open();
        var root = Assert.IsType<Grid>(page.Content);
        var overlay = Assert.Single(root.Children.OfType<AbsoluteLayout>());

        control.Close();

        Assert.Same(pageContent, page.Content);
        Assert.DoesNotContain(root.Children, child => ReferenceEquals(child, overlay));
        Assert.False(overlay.IsVisible);
        Assert.True(overlay.InputTransparent);
        Assert.Empty(overlay.Children);
    }

    [Fact]
    public void Open_ShouldUseExistingGridRootWithoutReparenting()
    {
        var pageRoot = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
            }
        };
        var pageContent = new VerticalStackLayout();
        pageRoot.Add(new Label { Text = "Header" }, row: 0);
        pageRoot.Add(pageContent, row: 1);
        var page = new ContentPage
        {
            Content = pageRoot
        };
        var control = new MauiDropdown
        {
            ItemsSource = new[] { "One", "Two" }
        };
        pageContent.Add(control);

        control.Open();
        var overlay = Assert.Single(pageRoot.Children.OfType<AbsoluteLayout>());

        Assert.Same(pageRoot, page.Content);
        Assert.Equal(2, Grid.GetRowSpan(overlay));

        control.Close();
        Assert.False(control.IsDropDownOpen);
        Assert.True(control.IsEnabled);
        Assert.Same(pageRoot, page.Content);
        Assert.DoesNotContain(pageRoot.Children, child => ReferenceEquals(child, overlay));

        control.Open();
        Assert.True(control.IsDropDownOpen);

        Assert.Same(pageRoot, page.Content);
        Assert.Same(overlay, Assert.Single(pageRoot.Children.OfType<AbsoluteLayout>()));
        Assert.True(overlay.IsVisible);
        Assert.False(overlay.InputTransparent);
    }

    [Fact]
    public void Open_ShouldUseUnstyledDismissLayer_WithLowOpacityBackdrop()
    {
        var pageContent = new VerticalStackLayout();
        var page = new ContentPage
        {
            Content = pageContent
        };
        var control = new MauiDropdown
        {
            ItemsSource = new[] { "One", "Two" }
        };
        pageContent.Add(control);

        control.Open();

        var root = Assert.IsType<Grid>(page.Content);
        var overlay = Assert.Single(root.Children.OfType<AbsoluteLayout>());
        var dismissLayer = Assert.IsType<Grid>(overlay.Children[0]);
        Assert.Equal(Colors.Black.WithAlpha(.05f), dismissLayer.BackgroundColor);

        control.Close();
    }

    [Fact]
    public void ItemContainer_ShouldApplyFeedbackBackgrounds()
    {
        var item = "One";
        var control = new TestMauiDropdown
        {
            SelectedItem = item,
            SelectedItemBackgroundColor = Colors.Red,
            HoveredItemBackgroundColor = Colors.Green,
            PressedItemBackgroundColor = Colors.Blue,
        };

        var container = Assert.IsType<StatefulContentView>(control.CreateContainer(item));

        Assert.Equal(Colors.Red, container.BackgroundColor);

        container.HoverCommand.Execute(null);
        Assert.Equal(Colors.Green, container.BackgroundColor);

        container.PressedCommand.Execute(null);
        Assert.Equal(Colors.Blue, container.BackgroundColor);

        container.HoverExitCommand.Execute(null);
        Assert.Equal(Colors.Red, container.BackgroundColor);
    }

    private sealed record TestItem(string Name);

    private sealed class MutableTestItem : INotifyPropertyChanged
    {
        private string name;

        public MutableTestItem(string name)
        {
            this.name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                {
                    return;
                }

                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    private static ContentView GetSelectedContent(MauiDropdown control)
    {
        var contentGrid = Assert.IsType<Grid>(control.Content);
        return Assert.IsType<ContentView>(contentGrid.Children[0]);
    }

    private static Path GetArrowPath(MauiDropdown control)
    {
        var contentGrid = Assert.IsType<Grid>(control.Content);
        return Assert.IsType<Path>(contentGrid.Children[1]);
    }

    private sealed class TestMauiDropdown : MauiDropdown
    {
        public View CreateItem(object item)
        {
            return CreateItemView(item);
        }

        public View CreateContainer(object item)
        {
            return CreateItemContainer(item);
        }
    }
}
