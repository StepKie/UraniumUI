using System.ComponentModel;
using UraniumUI.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Tests.Controls;

public class Select_Test
{
    public Select_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void ItemTemplate_ShouldCreateView_WithItemBindingContext()
    {
        var control = new TestSelect
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
        var control = new TestSelect
        {
            ItemDisplayBinding = new Binding(nameof(TestItem.Name))
        };

        var view = Assert.IsType<Label>(control.CreateItem(new TestItem("Grace")));

        Assert.Equal("Grace", view.Text);
    }

    [Fact]
    public void SelectedItem_ShouldUpdateClosedStateText()
    {
        var control = new Select
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
        var control = new Select
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
        var control = new Select
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
        var control = new Select
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
        var control = new Select
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
        var control = new Select
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
        var control = new Select
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
        var control = new Select();

        var arrowPath = GetArrowPath(control);

        Assert.NotNull(arrowPath.Data);
    }

    [Fact]
    public void Constructor_ShouldBeFocusable_ForKeyboardNavigation()
    {
        var control = new Select();

        Assert.True(control.IsFocusable);
        Assert.NotNull(control.TappedCommand);
    }

    [Fact]
    public void Placeholder_ShouldUpdateSemanticDescription()
    {
        var control = new Select
        {
            Placeholder = "Choose a profile"
        };

        Assert.Equal("Choose a profile", SemanticProperties.GetDescription(control));
    }

    [Fact]
    public void SelectedItem_ShouldUpdateSemanticDescription()
    {
        var control = new Select
        {
            ItemDisplayBinding = new Binding(nameof(TestItem.Name)),
            SelectedItem = new TestItem("Ada")
        };

        Assert.Equal("Ada", SemanticProperties.GetDescription(control));
    }

    [Fact]
    public void KeyboardEnter_ShouldOpenDropdown()
    {
        var control = CreateSelectOnPage("One", "Two");

        var handled = control.SendKeyDown(StatefulContentViewKey.Enter);

        Assert.True(handled);
        Assert.True(control.IsDropDownOpen);

        control.Close();
    }

    [Fact]
    public void KeyboardArrowDownAndEnter_ShouldSelectActiveItem()
    {
        var control = CreateSelectOnPage("One", "Two", "Three");

        control.Open();
        control.SendKeyDown(StatefulContentViewKey.ArrowDown);
        control.SendKeyDown(StatefulContentViewKey.Enter);

        Assert.Equal("Two", control.SelectedItem);
        Assert.False(control.IsDropDownOpen);
    }

    [Fact]
    public void KeyboardEscape_ShouldCloseDropdown()
    {
        var control = CreateSelectOnPage("One", "Two");
        control.Open();

        var handled = control.SendKeyDown(StatefulContentViewKey.Escape);

        Assert.True(handled);
        Assert.False(control.IsDropDownOpen);
    }

    [Fact]
    public void OpenAndClose_ShouldRotateArrow()
    {
        var pageContent = new VerticalStackLayout();
        var control = new Select
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
        var control = new Select
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
    public void OpenCloseOpen_ShouldRewrapNonGridContentWithoutDuplicatingContent()
    {
        var pageContent = new VerticalStackLayout();
        var page = new ContentPage
        {
            Content = pageContent
        };
        var control = new Select
        {
            ItemsSource = new[] { "One", "Two" }
        };
        pageContent.Add(control);

        control.Open();
        var root = Assert.IsType<Grid>(page.Content);

        control.Close();
        Assert.Same(pageContent, page.Content);

        control.Open();

        Assert.Same(root, page.Content);
        Assert.Equal(1, root.Children.Count(child => ReferenceEquals(child, pageContent)));
        Assert.Same(pageContent, root.Children[0]);

        control.Close();
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
        var control = new Select
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
    public void Open_ShouldPositionPopupRelativeToNestedSidePanel()
    {
        var pageRoot = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(7, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
            }
        };
        var preview = new ContentView();
        var sidePanel = new VerticalStackLayout();
        var control = new Select
        {
            ItemsSource = new[] { "Default", "Numeric", "Email" },
            WidthRequest = 180,
            HeightRequest = 40,
        };
        sidePanel.Add(control);
        pageRoot.Add(preview);
        pageRoot.Add(sidePanel, column: 1);
        _ = new ContentPage
        {
            Content = pageRoot
        };
        pageRoot.Arrange(new Rect(0, 0, 1000, 600));
        preview.Arrange(new Rect(0, 0, 700, 600));
        sidePanel.Arrange(new Rect(700, 0, 300, 600));
        control.Arrange(new Rect(20, 80, 180, 40));

        control.Open();

        var overlay = Assert.Single(pageRoot.Children.OfType<AbsoluteLayout>());
        var popup = Assert.Single(overlay.Children.OfType<Border>());
        var popupBounds = AbsoluteLayout.GetLayoutBounds(popup);

        Assert.True(popupBounds.X >= 700);
        Assert.True(popupBounds.Y > 0);

        control.Close();
    }

    [Fact]
    public void HostShow_ShouldPositionPopupRelativeToWrappedNonGridRoot()
    {
        var pageContent = new VerticalStackLayout();
        var control = new Select
        {
            WidthRequest = 180,
            HeightRequest = 40,
        };
        var page = new ContentPage
        {
            Content = pageContent
        };
        pageContent.Add(control);
        var host = new PopupOverlayHost(page);
        var root = Assert.IsType<Grid>(page.Content);
        var popup = new Border();

        root.Arrange(new Rect(0, 0, 1000, 700));
        pageContent.Arrange(new Rect(0, 0, 1000, 700));
        control.Arrange(new Rect(650, 520, 180, 40));

        var registration = host.Show(control, popup, new PopupOverlayOptions
        {
            Width = 180,
            MaxHeight = 240,
            Margin = new Thickness(0, 4),
        });

        var overlay = Assert.Single(root.Children.OfType<AbsoluteLayout>());
        var popupBounds = AbsoluteLayout.GetLayoutBounds(popup);

        Assert.Same(pageContent, root.Children[0]);
        Assert.Contains(popup, overlay.Children);
        Assert.True(popupBounds.X >= 650);
        Assert.True(popupBounds.Y > 250);

        registration.Close();
    }

    [Fact]
    public void Open_ShouldUseUnstyledDismissLayer_WithLowOpacityBackdrop()
    {
        var pageContent = new VerticalStackLayout();
        var page = new ContentPage
        {
            Content = pageContent
        };
        var control = new Select
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
    public void Open_ShouldNotUseNegativePopupWidth_WhenMarginsExceedRootWidth()
    {
        var pageRoot = new Grid();
        var page = new ContentPage
        {
            Content = pageRoot
        };
        var control = new Select
        {
            ItemsSource = new[] { "One", "Two" },
            DropDownMargin = new Thickness(40, 4),
            WidthRequest = 20,
            HeightRequest = 30,
        };
        pageRoot.Add(control);
        pageRoot.Arrange(new Rect(0, 0, 50, 200));
        control.Arrange(new Rect(10, 10, 20, 30));

        control.Open();

        var overlay = Assert.Single(pageRoot.Children.OfType<AbsoluteLayout>());
        var popup = Assert.Single(overlay.Children.OfType<Border>());
        var popupBounds = AbsoluteLayout.GetLayoutBounds(popup);

        Assert.True(popupBounds.Width >= 0);

        control.Close();
    }

    [Fact]
    public void ItemContainer_ShouldApplyFeedbackBackgrounds()
    {
        var item = "One";
        var control = new TestSelect
        {
            SelectedItem = item,
            SelectedItemBackgroundColor = Colors.Red,
            HoveredItemBackgroundColor = Colors.Green,
            PressedItemBackgroundColor = Colors.Blue,
        };

        var container = Assert.IsType<StatefulContentView>(control.CreateContainer(item));

        Assert.False(container.IsFocusable);
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

    private static ContentView GetSelectedContent(Select control)
    {
        var contentGrid = Assert.IsType<Grid>(control.Content);
        return Assert.IsType<ContentView>(contentGrid.Children[0]);
    }

    private static Path GetArrowPath(Select control)
    {
        var contentGrid = Assert.IsType<Grid>(control.Content);
        return Assert.IsType<Path>(contentGrid.Children[1]);
    }

    private static Select CreateSelectOnPage(params string[] items)
    {
        var pageContent = new VerticalStackLayout();
        var control = new Select
        {
            ItemsSource = items
        };
        _ = new ContentPage
        {
            Content = pageContent
        };
        pageContent.Add(control);
        return control;
    }

    private sealed class TestSelect : Select
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
