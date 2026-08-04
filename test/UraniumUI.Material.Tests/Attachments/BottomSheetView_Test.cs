using System.Reflection;
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

    [Fact]
    public void PresentationEvents_ShouldOnlyBeRaisedOnStateChanges()
    {
        var header = AnimationReadyHandler.Prepare(new Border());
        var control = AnimationReadyHandler.Prepare(new BottomSheetView
        {
            Header = header,
            Body = AnimationReadyHandler.Prepare(new BoxView()),
            CloseOnTapOutside = false,
            DisablePageWhenOpened = false,
        });
        control.OnAttached(new UraniumContentPage());
        SetFrame(control, 100, 300);
        var openedCount = 0;
        var closedCount = 0;
        control.Opened += (_, _) => openedCount++;
        control.Closed += (_, _) => closedCount++;

        control.IsPresented = true;

        openedCount.ShouldBe(1);
        closedCount.ShouldBe(0);

#pragma warning disable CS0618
        header.Layout(new Rect(0, 0, 100, 60));
#pragma warning restore CS0618

        openedCount.ShouldBe(1);
        closedCount.ShouldBe(0);

        control.IsPresented = false;

        openedCount.ShouldBe(1);
        closedCount.ShouldBe(1);

#pragma warning disable CS0618
        header.Layout(new Rect(0, 0, 100, 80));
#pragma warning restore CS0618

        openedCount.ShouldBe(1);
        closedCount.ShouldBe(1);
    }

    [Fact]
    public void TapOutside_ShouldRaiseClosed()
    {
        var page = new UraniumContentPage { Body = new Label() };
        var control = AnimationReadyHandler.Prepare(new BottomSheetView
        {
            Header = AnimationReadyHandler.Prepare(new Border()),
            Body = AnimationReadyHandler.Prepare(new BoxView()),
            DisablePageWhenOpened = false,
        });
        page.Attachments.Add(control);
        var closedCount = 0;
        control.Closed += (_, _) => closedCount++;

        control.IsPresented = true;

        var closeGesture = page.ContentFrame.GestureRecognizers.ShouldHaveSingleItem().ShouldBeOfType<TapGestureRecognizer>();
        var sendTapped = typeof(TapGestureRecognizer).GetMethod("SendTapped", BindingFlags.Instance | BindingFlags.NonPublic);
        sendTapped.ShouldNotBeNull();
        sendTapped.Invoke(closeGesture, new object[] { page.ContentFrame, null });

        control.IsPresented.ShouldBeFalse();
        closedCount.ShouldBe(1);
        page.ContentFrame.GestureRecognizers.ShouldBeEmpty();
    }

    [Fact]
    public void InitiallyPresentedSheet_ShouldRegisterTapOutsideAfterAttachment()
    {
        var page = new UraniumContentPage { Body = new Label() };
        var control = AnimationReadyHandler.Prepare(new BottomSheetView
        {
            Body = AnimationReadyHandler.Prepare(new BoxView()),
            DisablePageWhenOpened = false,
            IsPresented = true,
        });
        var closedCount = 0;
        control.Closed += (_, _) => closedCount++;

        page.Attachments.Add(control);

        var closeGesture = page.ContentFrame.GestureRecognizers.ShouldHaveSingleItem().ShouldBeOfType<TapGestureRecognizer>();
        var sendTapped = typeof(TapGestureRecognizer).GetMethod("SendTapped", BindingFlags.Instance | BindingFlags.NonPublic);
        sendTapped.ShouldNotBeNull();
        sendTapped.Invoke(closeGesture, new object[] { page.ContentFrame, null });

        control.IsPresented.ShouldBeFalse();
        closedCount.ShouldBe(1);
        page.ContentFrame.GestureRecognizers.ShouldBeEmpty();
    }

    private static void SetFrame(VisualElement element, double width, double height)
    {
        var frameProperty = typeof(VisualElement).GetProperty("Frame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        frameProperty.ShouldNotBeNull();
        frameProperty.SetValue(element, new Rect(0, 0, width, height));
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
