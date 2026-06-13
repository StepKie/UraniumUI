using Shouldly;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;

namespace UraniumUI.Material.Tests.Controls;

public class MauiDropdownField_Test
{
    public MauiDropdownField_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void SelectedItem_Binding_ToSource_UsesTwoWayByDefault()
    {
        var control = AnimationReadyHandler.Prepare(new MauiDropdownField());
        var viewModel = new TestViewModel { SelectedItem = "http://" };
        control.BindingContext = viewModel;
        control.SetBinding(MauiDropdownField.SelectedItemProperty, new Binding(nameof(TestViewModel.SelectedItem)));

        control.SelectedItem = "https://";

        viewModel.SelectedItem.ShouldBe(control.SelectedItem);
    }

    [Fact]
    public void ItemTemplate_ShouldBeForwarded_ToDropdownView()
    {
        var control = AnimationReadyHandler.Prepare(new MauiDropdownField());
        var itemTemplate = new DataTemplate(() => new Label());

        control.ItemTemplate = itemTemplate;

        control.DropdownView.ItemTemplate.ShouldBeSameAs(itemTemplate);
    }

    [Fact]
    public void SelectedItemTemplate_ShouldBeForwarded_ToDropdownView()
    {
        var control = AnimationReadyHandler.Prepare(new MauiDropdownField());
        var selectedItemTemplate = new DataTemplate(() => new Label());

        control.SelectedItemTemplate = selectedItemTemplate;

        control.DropdownView.SelectedItemTemplate.ShouldBeSameAs(selectedItemTemplate);
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

        var control = AnimationReadyHandler.Prepare(new MauiDropdownField());

        control.DropdownView.DropDownBackgroundColor.ShouldBe(Colors.Pink);
        control.DropdownView.DropDownBorderColor.ShouldBe(Colors.Blue);
        control.DropdownView.SelectedItemBackgroundColor.ShouldBe(Colors.Red);
        control.DropdownView.HoveredItemBackgroundColor.ShouldBe(Colors.Green);
        control.DropdownView.PressedItemBackgroundColor.ShouldBe(Colors.Orange.WithAlpha(.18f));
        control.DropdownView.TextColor.ShouldBe(Colors.Purple);
    }

    public class TestViewModel : UraniumBindableObject
    {
        private object selectedItem;

        public object SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }
    }
}
