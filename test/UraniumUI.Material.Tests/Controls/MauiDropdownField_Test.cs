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

    public class TestViewModel : UraniumBindableObject
    {
        private object selectedItem;

        public object SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }
    }
}
