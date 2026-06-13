using System.Runtime.CompilerServices;
using Microsoft.Maui.Layouts;

namespace UraniumUI.Controls;

internal sealed class PopupOverlayOptions
{
    public double Width { get; init; }

    public double MaxHeight { get; init; }

    public Thickness Margin { get; init; } = new(0, 4, 0, 4);
}

internal sealed class PopupOverlayRegistration : IDisposable
{
    private readonly Action close;
    private bool isClosed;

    public PopupOverlayRegistration(Action close)
    {
        this.close = close;
    }

    public event EventHandler Closed;

    public void Close()
    {
        if (isClosed)
        {
            return;
        }

        isClosed = true;
        close();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Close();
    }
}

internal sealed class PopupOverlayRootGrid : Grid
{
}

internal static class PopupOverlay
{
    private static readonly ConditionalWeakTable<Page, PopupOverlayHost> hosts = new();
    private static readonly ConditionalWeakTable<VisualElement, PopupOverlayHost> hostsByElement = new();

    public static PopupOverlayRegistration Show(VisualElement anchor, View popup, PopupOverlayOptions options)
    {
        if (anchor is null)
        {
            throw new ArgumentNullException(nameof(anchor));
        }

        if (popup is null)
        {
            throw new ArgumentNullException(nameof(popup));
        }

        var page = FindParentPage(anchor);
        if (page is not ContentPage contentPage)
        {
            return FindParentHost(anchor)?.Show(anchor, popup, options ?? new PopupOverlayOptions());
        }

        var host = hosts.GetValue(contentPage, page => new PopupOverlayHost((ContentPage)page));
        return host.Show(anchor, popup, options ?? new PopupOverlayOptions());
    }

    public static void Prepare(VisualElement anchor)
    {
        if (anchor is null)
        {
            return;
        }

        var page = FindParentPage(anchor, useApplicationFallback: false);
        if (page is ContentPage contentPage)
        {
            var host = hosts.GetValue(contentPage, page => new PopupOverlayHost((ContentPage)page));
            host.Prepare();
            return;
        }

        FindParentHost(anchor)?.Prepare();
    }

    internal static void RegisterHostElement(VisualElement element, PopupOverlayHost host)
    {
        hostsByElement.Remove(element);
        hostsByElement.Add(element, host);
    }

    private static Page FindParentPage(Element element, bool useApplicationFallback = true)
    {
        while (element is not null)
        {
            if (element is Page page)
            {
                return page;
            }

            element = element.Parent;
        }

        return useApplicationFallback ? Application.Current?.Windows.FirstOrDefault()?.Page : null;
    }

    private static PopupOverlayHost FindParentHost(Element element)
    {
        while (element is not null)
        {
            if (element is VisualElement visualElement && hostsByElement.TryGetValue(visualElement, out var host))
            {
                return host;
            }

            element = element.Parent;
        }

        return null;
    }
}

internal sealed class PopupOverlayHost
{
    private readonly ContentPage page;
    private Grid root;
    private AbsoluteLayout overlayLayer;
    private View originalContent;
    private bool ownsRoot;
    private PopupOverlayRegistration activeRegistration;
    private VisualElement activeAnchor;
    private View activeDismissLayer;
    private View activePopup;
    private PopupOverlayOptions activeOptions;
    private bool activePopupAnimated;
    private readonly List<ScrollView> activeScrollParents = new();

    public PopupOverlayHost(ContentPage page)
    {
        this.page = page;
        EnsureHost();
    }

    public void Prepare()
    {
        EnsureHost();
    }

    public PopupOverlayRegistration Show(VisualElement anchor, View popup, PopupOverlayOptions options)
    {
        CloseActive();
        EnsureHost();

        activeAnchor = anchor;
        activeDismissLayer = CreateDismissLayer();
        activePopup = popup;
        activeOptions = options;
        activePopupAnimated = false;

        AddOverlayLayerToRoot();

        overlayLayer.IsVisible = true;
        overlayLayer.InputTransparent = false;
        overlayLayer.Children.Add(activeDismissLayer);
        overlayLayer.Children.Add(popup);

        root.SizeChanged += OnRootSizeChanged;
        anchor.SizeChanged += OnAnchorSizeChanged;
        RegisterScrollParents(anchor);

        PopupOverlayRegistration registration = null;
        registration = new PopupOverlayRegistration(() =>
        {
            if (!ReferenceEquals(activeRegistration, registration))
            {
                return;
            }

            var activeOverlayLayer = overlayLayer;

            activeOverlayLayer.Children.Remove(popup);
            activeOverlayLayer.Children.Remove(activeDismissLayer);
            activeOverlayLayer.IsVisible = false;
            activeOverlayLayer.InputTransparent = true;
            root.Children.Remove(activeOverlayLayer);
            root.SizeChanged -= OnRootSizeChanged;
            anchor.SizeChanged -= OnAnchorSizeChanged;
            UnregisterScrollParents();

            ResetActivePopupState();
        });

        activeRegistration = registration;
        PositionPopup();
        DispatchPositionPopup();

        return registration;
    }

    private void EnsureHost()
    {
        if (TryUseCurrentHost() || TryUsePopupOverlayRootHost() || TryUsePageGridHost())
        {
            return;
        }

        WrapPageContent();
    }

    private bool TryUseCurrentHost()
    {
        if (root is null || overlayLayer is null)
        {
            return false;
        }

        if (ReferenceEquals(page.Content, root))
        {
            return true;
        }

        if (!ownsRoot || !ReferenceEquals(page.Content, originalContent))
        {
            return false;
        }

        RegisterHostElements();
        page.Content = root;
        AttachOriginalContentToRoot(insertAtStart: true);

        return true;
    }

    private bool TryUsePopupOverlayRootHost()
    {
        if (page.Content is not PopupOverlayRootGrid existingRoot)
        {
            return false;
        }

        root = existingRoot;
        overlayLayer = existingRoot.Children.OfType<AbsoluteLayout>().FirstOrDefault(x => x.StyleId == nameof(PopupOverlay)) ?? overlayLayer ?? CreateOverlayLayer();
        originalContent = existingRoot.Children.FirstOrDefault(x => !ReferenceEquals(x, overlayLayer)) as View;
        ownsRoot = true;
        RegisterHostElements();
        return true;
    }

    private bool TryUsePageGridHost()
    {
        if (page.Content is not Grid existingRoot)
        {
            return false;
        }

        root = existingRoot;
        overlayLayer = existingRoot.Children.OfType<AbsoluteLayout>().FirstOrDefault(x => x.StyleId == nameof(PopupOverlay)) ?? overlayLayer ?? CreateOverlayLayer();
        originalContent = existingRoot;
        ownsRoot = false;
        RegisterHostElements();
        return true;
    }

    private void WrapPageContent()
    {
        originalContent = page.Content;
        root = ownsRoot && root is not null ? root : CreateRoot();
        ownsRoot = true;

        overlayLayer ??= CreateOverlayLayer();

        RegisterHostElements();
        page.Content = root;
        AttachOriginalContentToRoot(insertAtStart: false);
    }

    private Grid CreateRoot()
    {
        return new PopupOverlayRootGrid
        {
            StyleId = nameof(PopupOverlayRootGrid),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    private void AttachOriginalContentToRoot(bool insertAtStart)
    {
        if (originalContent is null || root.Children.Contains(originalContent))
        {
            return;
        }

        if (ReferenceEquals(page.Content, originalContent))
        {
            page.Content = null;
        }

        if (insertAtStart)
        {
            root.Children.Insert(0, originalContent);
            return;
        }

        root.Children.Add(originalContent);
    }

    private AbsoluteLayout CreateOverlayLayer()
    {
        return new AbsoluteLayout
        {
            StyleId = nameof(PopupOverlay),
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            InputTransparent = true,
            IsVisible = false,
            ZIndex = 9999,
        };
    }

    private void AddOverlayLayerToRoot()
    {
        Grid.SetRow(overlayLayer, 0);
        Grid.SetColumn(overlayLayer, 0);
        Grid.SetRowSpan(overlayLayer, Math.Max(1, root.RowDefinitions.Count));
        Grid.SetColumnSpan(overlayLayer, Math.Max(1, root.ColumnDefinitions.Count));

        if (!root.Children.Contains(overlayLayer))
        {
            root.Children.Add(overlayLayer);
        }
    }

    private void RegisterHostElements()
    {
        PopupOverlay.RegisterHostElement(root, this);

        if (originalContent is not null)
        {
            PopupOverlay.RegisterHostElement(originalContent, this);
        }
    }

    private View CreateDismissLayer()
    {
        var dismissLayer = new Grid
        {
            BackgroundColor = Colors.Black.WithAlpha(.05f),
        };

        dismissLayer.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(CloseActive)
        });

        AbsoluteLayout.SetLayoutFlags(dismissLayer, AbsoluteLayoutFlags.All);
        AbsoluteLayout.SetLayoutBounds(dismissLayer, new Rect(0, 0, 1, 1));

        return dismissLayer;
    }

    private void CloseActive()
    {
        activeRegistration?.Close();
    }

    private void OnRootSizeChanged(object sender, EventArgs e)
    {
        PositionPopup();
    }

    private void OnAnchorSizeChanged(object sender, EventArgs e)
    {
        PositionPopup();
    }

    private void OnScrollParentScrolled(object sender, ScrolledEventArgs e)
    {
        PositionPopup();
    }

    private void DispatchPositionPopup()
    {
        DispatchPositionPopup(TimeSpan.FromMilliseconds(1));
        DispatchPositionPopup(TimeSpan.FromMilliseconds(16));
        DispatchPositionPopup(TimeSpan.FromMilliseconds(50));
    }

    private void DispatchPositionPopup(TimeSpan delay)
    {
        try
        {
            GetDispatcher()?.DispatchDelayed(delay, () => PositionPopup());
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // Unit tests can run without an application dispatcher.
        }
    }

    private IDispatcher GetDispatcher()
    {
        try
        {
            return root?.Dispatcher;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            return null;
        }
    }

    private void PositionPopup()
    {
        try
        {
            TryPositionPopup();
        }
        catch (ObjectDisposedException)
        {
            CancelActivePopup();
        }
    }

    private void TryPositionPopup()
    {
        var placementWidth = overlayLayer.Width > 0 ? overlayLayer.Width : root.Width;
        var placementHeight = overlayLayer.Height > 0 ? overlayLayer.Height : root.Height;

        if (activeAnchor is null || activePopup is null || activeOptions is null || placementWidth <= 0 || placementHeight <= 0)
        {
            return;
        }

        var anchorBounds = GetAnchorBoundsRelativeToOverlay();
        var availableWidth = Math.Max(0, placementWidth - activeOptions.Margin.HorizontalThickness);
        var requestedWidth = activeOptions.Width > 0 ? activeOptions.Width : anchorBounds.Width;
        var minimumWidth = Math.Min(anchorBounds.Width, availableWidth);
        var width = Math.Min(Math.Max(requestedWidth, minimumWidth), availableWidth);

        var x = Math.Min(Math.Max(activeOptions.Margin.Left, anchorBounds.X), Math.Max(activeOptions.Margin.Left, placementWidth - width - activeOptions.Margin.Right));

        var availableBelow = placementHeight - anchorBounds.Bottom - activeOptions.Margin.Bottom;
        var availableAbove = anchorBounds.Top - activeOptions.Margin.Top;
        var desiredMaxHeight = activeOptions.MaxHeight > 0 ? activeOptions.MaxHeight : 240d;
        var firstPassMaxHeight = Math.Max(0, Math.Min(desiredMaxHeight, Math.Max(availableBelow, availableAbove)));

        activePopup.MaximumHeightRequest = firstPassMaxHeight;
        var measuredHeight = activePopup.Measure(width, firstPassMaxHeight).Height;
        if (double.IsNaN(measuredHeight) || measuredHeight <= 0)
        {
            measuredHeight = firstPassMaxHeight;
        }

        var showAbove = availableBelow < measuredHeight && availableAbove > availableBelow;
        var availableHeight = Math.Max(0, showAbove ? availableAbove : availableBelow);
        var height = Math.Min(measuredHeight, Math.Min(desiredMaxHeight, availableHeight));
        activePopup.MaximumHeightRequest = height;

        var y = showAbove
            ? Math.Max(activeOptions.Margin.Top, anchorBounds.Top - height)
            : Math.Min(anchorBounds.Bottom, placementHeight - height - activeOptions.Margin.Bottom);

        AbsoluteLayout.SetLayoutFlags(activePopup, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(activePopup, new Rect(x, y, width, AbsoluteLayout.AutoSize));

        if (!activePopupAnimated)
        {
            activePopupAnimated = true;
            _ = AnimatePopup(showAbove);
        }
    }

    private void CancelActivePopup()
    {
        var registration = activeRegistration;

        if (registration is null)
        {
            return;
        }

        try
        {
            registration.Close();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // Navigation can dispose native services while delayed positioning callbacks are still queued.
            ResetActivePopupState();
        }
    }

    private void ResetActivePopupState()
    {
        activeRegistration = null;
        activeAnchor = null;
        activeDismissLayer = null;
        activePopup = null;
        activeOptions = null;
        activePopupAnimated = false;
    }

    private async Task AnimatePopup(bool showAbove)
    {
        var popup = activePopup;

        if (popup is null)
        {
            return;
        }

        if (activeAnchor?.Handler is null || popup.Handler is null)
        {
            popup.Opacity = 1;
            popup.Scale = 1;
            popup.TranslationY = 0;
            return;
        }

        popup.Opacity = 0;
        popup.Scale = .97;
        popup.TranslationY = showAbove ? 6 : -6;
        popup.AnchorX = .5;
        popup.AnchorY = showAbove ? 1 : 0;

        try
        {
            await Task.WhenAll(
                popup.FadeTo(1, 120, Easing.CubicOut),
                popup.ScaleTo(1, 120, Easing.CubicOut),
                popup.TranslateTo(0, 0, 120, Easing.CubicOut));
        }
        catch
        {
            // The popup may be detached while this visual-only animation is still running.
        }
        finally
        {
            popup.Opacity = 1;
            popup.Scale = 1;
            popup.TranslationY = 0;
        }
    }

    private void RegisterScrollParents(Element element)
    {
        UnregisterScrollParents();

        var current = element.Parent;
        while (current is not null)
        {
            if (current is ScrollView scrollView && !activeScrollParents.Contains(scrollView))
            {
                scrollView.Scrolled += OnScrollParentScrolled;
                activeScrollParents.Add(scrollView);
            }

            current = current.Parent;
        }
    }

    private void UnregisterScrollParents()
    {
        foreach (var scrollView in activeScrollParents)
        {
            scrollView.Scrolled -= OnScrollParentScrolled;
        }

        activeScrollParents.Clear();
    }

    private Rect GetAnchorBoundsRelativeToOverlay()
    {
        var anchorBounds = GetBoundsRelativeTo(activeAnchor, root);
        var overlayRootBounds = GetBoundsRelativeTo(overlayLayer, root);
        return new Rect(
            anchorBounds.X - overlayRootBounds.X,
            anchorBounds.Y - overlayRootBounds.Y,
            anchorBounds.Width,
            anchorBounds.Height);
    }

    internal static Rect GetBoundsRelativeTo(VisualElement element, VisualElement relativeTo)
    {
        var x = 0d;
        var y = 0d;
        var current = element;

        while (current is not null && !ReferenceEquals(current, relativeTo))
        {
            x += current.X + current.TranslationX;
            y += current.Y + current.TranslationY;

            if (current.Parent is ScrollView scrollView)
            {
                x -= scrollView.ScrollX;
                y -= scrollView.ScrollY;
            }

            current = current.Parent as VisualElement;
        }

        return new Rect(x, y, element.Width, element.Height);
    }
}
