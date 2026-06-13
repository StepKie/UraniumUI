using UraniumUI.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Views;

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
        var selectedLabel = ((Grid)control.Content).Children.OfType<Label>().First();

        Assert.Equal("Choose", selectedLabel.Text);

        control.SelectedItem = new TestItem("Katherine");

        Assert.Equal("Katherine", selectedLabel.Text);
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
