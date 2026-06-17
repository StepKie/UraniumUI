using Shouldly;
using System.ComponentModel;
using UraniumUI.Controls;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Tests.Controls;

public class SelectField_Test
{
    public SelectField_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void SelectedItem_Binding_ToSource_UsesTwoWayByDefault()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var viewModel = new TestViewModel { SelectedItem = "http://" };
        control.BindingContext = viewModel;
        control.SetBinding(SelectField.SelectedItemProperty, new Binding(nameof(TestViewModel.SelectedItem)));

        control.SelectedItem = "https://";

        viewModel.SelectedItem.ShouldBe(control.SelectedItem);
    }

    [Fact]
    public void ItemTemplate_ShouldBeForwarded_ToSelectView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var itemTemplate = new DataTemplate(() => new Label());

        control.ItemTemplate = itemTemplate;

        control.SelectView.ItemTemplate.ShouldBeSameAs(itemTemplate);
    }

    [Fact]
    public void SelectedItemTemplate_ShouldBeForwarded_ToSelectView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var selectedItemTemplate = new DataTemplate(() => new Label());

        control.SelectedItemTemplate = selectedItemTemplate;

        control.SelectView.SelectedItemTemplate.ShouldBeSameAs(selectedItemTemplate);
    }

    [Fact]
    public void ItemDisplayBinding_ShouldBeForwarded_ToSelectView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var itemDisplayBinding = new Binding(nameof(DisplayTestItem.Name));

        control.ItemDisplayBinding = itemDisplayBinding;

        control.SelectView.ItemDisplayBinding.ShouldBeSameAs(itemDisplayBinding);
    }

    [Fact]
    public void SelectedItem_ShouldUseItemDisplayBinding_ForClosedText()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var item = new DisplayTestItem("Ada Lovelace");

        control.ItemDisplayBinding = new Binding(nameof(DisplayTestItem.Name));
        control.SelectedItem = item;

        GetSelectedLabel(control.SelectView).Text.ShouldBe("Ada Lovelace");
    }

    [Fact]
    public void Constructor_ShouldUseFocusableSelectView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());

        control.SelectView.IsFocusable.ShouldBeTrue();
        control.SelectView.StyleClass.ShouldContain("InputField.Select");
    }

    [Fact]
    public void Open_ShouldPositionPopupRelativeToSelectField()
    {
        var pageRoot = new Grid();
        var control = AnimationReadyHandler.Prepare(new SelectField
        {
            WidthRequest = 240,
            HeightRequest = 55,
        });
        control.ItemsSource = new[] { "Default", "Numeric", "Email" };
        _ = new ContentPage
        {
            Content = pageRoot
        };
        pageRoot.Add(control);
        pageRoot.Arrange(new Rect(0, 0, 1000, 700));
        control.Arrange(new Rect(650, 520, 240, 55));
        control.SelectView.Arrange(new Rect(0, 0, 240, 45));

        control.SelectView.Open();

        var overlay = pageRoot.Children.OfType<AbsoluteLayout>().Single();
        var popup = overlay.Children.OfType<Border>().Single();
        var popupBounds = AbsoluteLayout.GetLayoutBounds(popup);

        popupBounds.X.ShouldBeGreaterThanOrEqualTo(650);
        popupBounds.Y.ShouldBeGreaterThan(250);

        control.Close();
    }

    [Fact]
    public void AllowClear_ShouldExposeSemanticDescription_ForClearButton()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());

        control.AllowClear = true;

        var clearButton = control.Attachments.OfType<StatefulContentView>().Single();

        SemanticProperties.GetDescription(clearButton).ShouldBe("Clear selection");
        SemanticProperties.GetHint(clearButton).ShouldBe("Clears the selected value.");
    }

    [Fact]
    public void SelectedItem_ShouldSyncBetweenSelectAndField_WhenBoundToSameReactiveSource()
    {
        var select = AnimationReadyHandler.Prepare(new Select());
        var field = AnimationReadyHandler.Prepare(new SelectField());
        AnimationReadyHandler.Prepare(field.SelectView);
        var viewModel = new ReactiveTestViewModel();
        select.BindingContext = viewModel;
        field.BindingContext = viewModel;
        select.SetBinding(Select.SelectedItemProperty, new Binding(nameof(ReactiveTestViewModel.SelectedItem)));
        field.SetBinding(SelectField.SelectedItemProperty, new Binding(nameof(ReactiveTestViewModel.SelectedItem)));

        select.SelectedItem = "Ada";

        viewModel.SelectedItem.ShouldBe("Ada");
        field.SelectedItem.ShouldBe("Ada");

        field.SelectedItem = "Grace";

        viewModel.SelectedItem.ShouldBe("Grace");
        select.SelectedItem.ShouldBe("Grace");
    }

    [Fact]
    public void Constructor_ShouldApplyMaterialThemeColors_ToSelectView()
    {
        Application.Current.UserAppTheme = AppTheme.Light;
        Application.Current.Resources["Surface"] = Colors.Pink;
        Application.Current.Resources["Outline"] = Colors.Blue;
        Application.Current.Resources["PrimaryContainer"] = Colors.Red;
        Application.Current.Resources["SurfaceVariant"] = Colors.Green;
        Application.Current.Resources["Primary"] = Colors.Orange;
        Application.Current.Resources["OnBackground"] = Colors.Purple;

        var control = AnimationReadyHandler.Prepare(new SelectField());

        control.SelectView.DropDownBackgroundColor.ShouldBe(Colors.Pink);
        control.SelectView.DropDownBorderColor.ShouldBe(Colors.Blue);
        control.SelectView.SelectedItemBackgroundColor.ShouldBe(Colors.Red);
        control.SelectView.HoveredItemBackgroundColor.ShouldBe(Colors.Green);
        control.SelectView.PressedItemBackgroundColor.ShouldBe(Colors.Orange.WithAlpha(.18f));
        control.SelectView.TextColor.ShouldBe(Colors.Purple);
        control.SelectView.PlaceholderColor.ShouldBe(Colors.Purple.WithAlpha(.5f));
    }

    [Fact]
    public void Constructor_ShouldApplyMaterialThemePlaceholderColor_ToSelectArrow()
    {
        var originalTheme = Application.Current.UserAppTheme;

        try
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            Application.Current.Resources["OnBackgroundDark"] = Colors.White;

            var control = AnimationReadyHandler.Prepare(new SelectField());
            var arrowPath = GetArrowPath(control.SelectView);

            var arrowBrush = arrowPath.Fill.ShouldBeOfType<SolidColorBrush>();
            arrowBrush.Color.ShouldBe(Colors.White.WithAlpha(.5f));
        }
        finally
        {
            Application.Current.UserAppTheme = originalTheme;
        }
    }

    public class TestViewModel : UraniumBindableObject
    {
        private object selectedItem;

        public object SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }
    }

    public class ReactiveTestViewModel : INotifyPropertyChanged
    {
        private object selectedItem;

        public event PropertyChangedEventHandler PropertyChanged;

        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value)
                {
                    return;
                }

                selectedItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }
    }

    private sealed record DisplayTestItem(string Name);

    private static Label GetSelectedLabel(UraniumUI.Controls.Select control)
    {
        var contentGrid = control.Content.ShouldBeOfType<Grid>();
        var selectedContent = contentGrid.Children[0].ShouldBeOfType<ContentView>();
        return selectedContent.Content.ShouldBeOfType<Label>();
    }

    private static Path GetArrowPath(UraniumUI.Controls.Select control)
    {
        var contentGrid = control.Content.ShouldBeOfType<Grid>();
        return contentGrid.Children[1].ShouldBeOfType<Path>();
    }
}
