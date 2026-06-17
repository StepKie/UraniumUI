using System.Reflection;
using Shouldly;
using UraniumUI.Material.Attachments;
using UraniumUI.Pages;
using UraniumUI.Tests.Core;

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
    public void HeaderSizeChanged_ShouldRealignCollapsedSheet()
    {
        var header = AnimationReadyHandler.Prepare(new Border());
        var body = AnimationReadyHandler.Prepare(new BoxView());
        var control = AnimationReadyHandler.Prepare(new BottomSheetView
        {
            Header = header,
            Body = body,
            CloseOnTapOutside = false,
            DisablePageWhenOpened = false,
        });

        control.OnAttached(new UraniumContentPage());
        SetFrame(control, 100, 300);
#pragma warning disable CS0618 // Layout is the simplest way to trigger SizeChanged in the headless MAUI test host.
        header.Layout(new Rect(0, 0, 100, 200));
#pragma warning restore CS0618

        control.IsPresented = true;
        control.TranslationY.ShouldBe(0);

        control.IsPresented = false;
        control.TranslationY.ShouldBe(100);

#pragma warning disable CS0618 // Layout is the simplest way to trigger SizeChanged in the headless MAUI test host.
        header.Layout(new Rect(0, 0, 100, 60));
#pragma warning restore CS0618

        control.TranslationY.ShouldBe(240);
    }

    private static void SetFrame(VisualElement element, double width, double height)
    {
        var frameProperty = typeof(VisualElement).GetProperty("Frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        frameProperty.ShouldNotBeNull();
        frameProperty.SetValue(element, new Rect(0, 0, width, height));
    }

    internal class BottomSheetTestViewModel : UraniumBindableObject
    {
        private bool disablePageWhenOpened;
        public bool DisablePageWhenOpened { get => disablePageWhenOpened; set => SetProperty(ref disablePageWhenOpened, value); }
    }
}
