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
    public void ItemTemplate_ShouldBeForwarded_ToDropdownView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var itemTemplate = new DataTemplate(() => new Label());

        control.ItemTemplate = itemTemplate;

        control.DropdownView.ItemTemplate.ShouldBeSameAs(itemTemplate);
    }

    [Fact]
    public void SelectedItemTemplate_ShouldBeForwarded_ToDropdownView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var selectedItemTemplate = new DataTemplate(() => new Label());

        control.SelectedItemTemplate = selectedItemTemplate;

        control.DropdownView.SelectedItemTemplate.ShouldBeSameAs(selectedItemTemplate);
    }

    [Fact]
    public void ItemDisplayBinding_ShouldBeForwarded_ToDropdownView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var itemDisplayBinding = new Binding(nameof(DisplayTestItem.Name));

        control.ItemDisplayBinding = itemDisplayBinding;

        control.DropdownView.ItemDisplayBinding.ShouldBeSameAs(itemDisplayBinding);
    }

    [Fact]
    public void SelectedItem_ShouldUseItemDisplayBinding_ForClosedText()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());
        var item = new DisplayTestItem("Ada Lovelace");

        control.ItemDisplayBinding = new Binding(nameof(DisplayTestItem.Name));
        control.SelectedItem = item;

        GetSelectedLabel(control.DropdownView).Text.ShouldBe("Ada Lovelace");
    }

    [Fact]
    public void Constructor_ShouldUseFocusableDropdownView()
    {
        var control = AnimationReadyHandler.Prepare(new SelectField());

        control.DropdownView.IsFocusable.ShouldBeTrue();
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
    public void SelectedItem_ShouldSyncBetweenDropdownAndField_WhenBoundToSameReactiveSource()
    {
        var dropdown = AnimationReadyHandler.Prepare(new Select());
        var field = AnimationReadyHandler.Prepare(new SelectField());
        AnimationReadyHandler.Prepare(field.DropdownView);
        var viewModel = new ReactiveTestViewModel();
        dropdown.BindingContext = viewModel;
        field.BindingContext = viewModel;
        dropdown.SetBinding(Select.SelectedItemProperty, new Binding(nameof(ReactiveTestViewModel.SelectedItem)));
        field.SetBinding(SelectField.SelectedItemProperty, new Binding(nameof(ReactiveTestViewModel.SelectedItem)));

        dropdown.SelectedItem = "Ada";

        viewModel.SelectedItem.ShouldBe("Ada");
        field.SelectedItem.ShouldBe("Ada");

        field.SelectedItem = "Grace";

        viewModel.SelectedItem.ShouldBe("Grace");
        dropdown.SelectedItem.ShouldBe("Grace");
    }

    [Fact]
    public void Constructor_ShouldApplyMaterialThemeColors_ToDropdownView()
    {
        Application.Current.UserAppTheme = AppTheme.Light;
        Application.Current.Resources["Surface"] = Colors.Pink;
        Application.Current.Resources["Outline"] = Colors.Blue;
        Application.Current.Resources["PrimaryContainer"] = Colors.Red;
        Application.Current.Resources["SurfaceVariant"] = Colors.Green;
        Application.Current.Resources["Primary"] = Colors.Orange;
        Application.Current.Resources["OnBackground"] = Colors.Purple;

        var control = AnimationReadyHandler.Prepare(new SelectField());

        control.DropdownView.DropDownBackgroundColor.ShouldBe(Colors.Pink);
        control.DropdownView.DropDownBorderColor.ShouldBe(Colors.Blue);
        control.DropdownView.SelectedItemBackgroundColor.ShouldBe(Colors.Red);
        control.DropdownView.HoveredItemBackgroundColor.ShouldBe(Colors.Green);
        control.DropdownView.PressedItemBackgroundColor.ShouldBe(Colors.Orange.WithAlpha(.18f));
        control.DropdownView.TextColor.ShouldBe(Colors.Purple);
        control.DropdownView.PlaceholderColor.ShouldBe(Colors.Purple.WithAlpha(.5f));
    }

    [Fact]
    public void Constructor_ShouldApplyMaterialThemePlaceholderColor_ToDropdownArrow()
    {
        var originalTheme = Application.Current.UserAppTheme;

        try
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            Application.Current.Resources["OnBackgroundDark"] = Colors.White;

            var control = AnimationReadyHandler.Prepare(new SelectField());
            var arrowPath = GetArrowPath(control.DropdownView);

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
