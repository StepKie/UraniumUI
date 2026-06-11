using Shouldly;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Views;

namespace UraniumUI.Material.Tests.Controls;

public class DropdownField_Test
{
    public DropdownField_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void ClearIcon_HasAsymmetricLeftHitPadding()
    {
        var control = AnimationReadyHandler.Prepare(new DropdownField());

        control.AllowClear = true;

        var clearIcon = control.Attachments.OfType<StatefulContentView>().Single();

        clearIcon.Margin.ShouldBe(default(Thickness));
        clearIcon.Padding.ShouldBe(new Thickness(InputField.BuiltInAttachmentLeftPadding, 0, 0, 0));
    }

    [Fact]
    public void SelectedItem_Binding_ToSource_UsesTwoWayByDefault()
    {
        var control = AnimationReadyHandler.Prepare(new DropdownField());
        var viewModel = new TestViewModel { SelectedItem = "http://" };
        control.BindingContext = viewModel;
        control.SetBinding(DropdownField.SelectedItemProperty, new Binding(nameof(TestViewModel.SelectedItem)));

        control.SelectedItem = "https://";

        viewModel.SelectedItem.ShouldBe(control.SelectedItem);
    }

    public class TestViewModel : UraniumBindableObject
    {
        private object selectedItem;

        public object SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }
    }
}
