using Shouldly;
using UraniumUI.Material.Attachments;
using UraniumUI.Pages;
using UraniumUI.Tests.Core;
using UraniumUI.Views;

namespace UraniumUI.Material.Tests.Attachments;
public class BottomSheetView_Test
{
    public BottomSheetView_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void DisablePageWhenOpened_BindingForInitialization_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new BottomSheetView());
        var viewModel = new { DisablePageWhenOpened = false };
        control.BindingContext = viewModel;
        control.SetBinding(BottomSheetView.DisablePageWhenOpenedProperty, new Binding(nameof(viewModel.DisablePageWhenOpened)));

        // Assert
        control.DisablePageWhenOpened.ShouldBe(viewModel.DisablePageWhenOpened);
    }

    [Fact]
    public void DisablePageWhenOpened_ShouldBeChanged_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new BottomSheetView());
        var viewModel = new BottomSheetTestViewModel { DisablePageWhenOpened = false };
        control.BindingContext = viewModel;
        control.SetBinding(BottomSheetView.DisablePageWhenOpenedProperty, new Binding(nameof(viewModel.DisablePageWhenOpened)));

        // Act
        viewModel.DisablePageWhenOpened = true;

        // Assert
        control.DisablePageWhenOpened.ShouldBe(viewModel.DisablePageWhenOpened);
    }

    [Fact]
    public void GeneratedHeader_ShouldBeFocusableAndExposeExpandedStateSemantics()
    {
        var page = new UraniumContentPage();
        var control = new BottomSheetView { Body = new Label { Text = "Body" } };

        page.Attachments.Add(control);

        var header = control.Header.ShouldBeOfType<StatefulContentView>();
        header.IsFocusable.ShouldBeTrue();
        SemanticProperties.GetDescription(header).ShouldBe("Expand bottom sheet");
        SemanticProperties.GetHint(header).ShouldBe("Toggles the bottom sheet.");

        control.IsPresented = true;

        SemanticProperties.GetDescription(header).ShouldBe("Collapse bottom sheet");
    }

    internal class BottomSheetTestViewModel : UraniumBindableObject
    {
        private bool disablePageWhenOpened;
        public bool DisablePageWhenOpened { get => disablePageWhenOpened; set => SetProperty(ref disablePageWhenOpened, value); }
    }
}
