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
        var contentGrid = Assert.IsType<Grid>(control.Content);

        var arrowPath = Assert.IsType<Path>(contentGrid.Children[1]);

        Assert.NotNull(arrowPath.Data);
    }

    [Fact]
    public void Close_ShouldRestoreOriginalPageContent()
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
        control.Close();

        Assert.Same(pageContent, page.Content);
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

    private static ContentView GetSelectedContent(MauiDropdown control)
    {
        var contentGrid = Assert.IsType<Grid>(control.Content);
        return Assert.IsType<ContentView>(contentGrid.Children[0]);
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
