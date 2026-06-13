using System.Runtime.CompilerServices;
using Microsoft.Maui.Layouts;

namespace UraniumUI.Controls;

internal sealed class DropdownOverlayOptions
{
    public double Width { get; init; }

    public double MaxHeight { get; init; }

    public Thickness Margin { get; init; } = new(0, 4, 0, 4);
}

internal sealed class DropdownOverlayRegistration : IDisposable
{
    private readonly Action close;
    private bool isClosed;

    public DropdownOverlayRegistration(Action close)
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

internal static class DropdownOverlay
{
    private static readonly ConditionalWeakTable<Page, DropdownOverlayHost> hosts = new();

    public static DropdownOverlayRegistration Show(VisualElement anchor, View popup, DropdownOverlayOptions options)
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
            return null;
        }

        var host = hosts.GetValue(contentPage, page => new DropdownOverlayHost((ContentPage)page));
        return host.Show(anchor, popup, options ?? new DropdownOverlayOptions());
    }

    private static Page FindParentPage(Element element)
    {
        while (element is not null)
        {
            if (element is Page page)
            {
                return page;
            }

            element = element.Parent;
        }

        return Application.Current?.Windows.FirstOrDefault()?.Page;
    }
}

internal sealed class DropdownOverlayHost
{
    private readonly ContentPage page;
    private Grid root;
    private AbsoluteLayout overlayLayer;
    private View originalContent;
    private DropdownOverlayRegistration activeRegistration;
    private VisualElement activeAnchor;
    private BoxView activeDismissLayer;
    private View activePopup;
    private DropdownOverlayOptions activeOptions;

    public DropdownOverlayHost(ContentPage page)
    {
        this.page = page;
        EnsureHost();
    }

    public DropdownOverlayRegistration Show(VisualElement anchor, View popup, DropdownOverlayOptions options)
    {
        CloseActive();
        EnsureHost();

        activeAnchor = anchor;
        activeDismissLayer = CreateDismissLayer();
        activePopup = popup;
        activeOptions = options;

        if (!root.Children.Contains(overlayLayer))
        {
            root.Children.Add(overlayLayer);
        }

        overlayLayer.IsVisible = true;
        overlayLayer.InputTransparent = false;
        overlayLayer.Children.Add(activeDismissLayer);
        overlayLayer.Children.Add(popup);

        root.SizeChanged += OnRootSizeChanged;
        anchor.SizeChanged += OnAnchorSizeChanged;
        PositionPopup();

        DropdownOverlayRegistration registration = null;
        registration = new DropdownOverlayRegistration(() =>
        {
            if (!ReferenceEquals(activeRegistration, registration))
            {
                return;
            }

            var activeRoot = root;
            var activeOverlayLayer = overlayLayer;
            var activeOriginalContent = originalContent;

            activeOverlayLayer.Children.Remove(popup);
            activeOverlayLayer.Children.Remove(activeDismissLayer);
            activeOverlayLayer.IsVisible = false;
            activeOverlayLayer.InputTransparent = true;
            activeRoot.Children.Remove(activeOverlayLayer);
            root.SizeChanged -= OnRootSizeChanged;
            anchor.SizeChanged -= OnAnchorSizeChanged;

            if (ReferenceEquals(page.Content, activeRoot))
            {
                if (activeOriginalContent is not null)
                {
                    activeRoot.Children.Remove(activeOriginalContent);
                }

                page.Content = activeOriginalContent;
            }

            activeRegistration = null;
            activeAnchor = null;
            activeDismissLayer = null;
            activePopup = null;
            activeOptions = null;
            originalContent = null;
            overlayLayer = null;
            root = null;
        });

        activeRegistration = registration;
        return registration;
    }

    private void EnsureHost()
    {
        if (root is not null && overlayLayer is not null && ReferenceEquals(page.Content, root))
        {
            return;
        }

        if (page.Content is Grid existingRoot && existingRoot.Children.OfType<AbsoluteLayout>().FirstOrDefault(x => x.StyleId == nameof(DropdownOverlay)) is AbsoluteLayout existingOverlay)
        {
            root = existingRoot;
            overlayLayer = existingOverlay;
            originalContent = existingRoot.Children.OfType<View>().FirstOrDefault(x => !ReferenceEquals(x, existingOverlay));
            return;
        }

        originalContent = page.Content;
        root = new Grid();

        if (originalContent is not null)
        {
            root.Children.Add(originalContent);
        }

        overlayLayer = new AbsoluteLayout
        {
            StyleId = nameof(DropdownOverlay),
            BackgroundColor = Colors.Transparent,
            InputTransparent = true,
            IsVisible = false,
            ZIndex = 9999,
        };

        page.Content = root;
    }

    private BoxView CreateDismissLayer()
    {
        var dismissLayer = new BoxView
        {
            BackgroundColor = Colors.Transparent,
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

    private void PositionPopup()
    {
        if (activeAnchor is null || activePopup is null || activeOptions is null || root.Width <= 0 || root.Height <= 0)
        {
            return;
        }

        var anchorBounds = GetBoundsRelativeTo(activeAnchor, root);
        var width = activeOptions.Width > 0 ? activeOptions.Width : anchorBounds.Width;
        width = Math.Min(Math.Max(width, anchorBounds.Width), root.Width - activeOptions.Margin.HorizontalThickness);

        var x = Math.Min(Math.Max(activeOptions.Margin.Left, anchorBounds.X), Math.Max(activeOptions.Margin.Left, root.Width - width - activeOptions.Margin.Right));

        var availableBelow = root.Height - anchorBounds.Bottom - activeOptions.Margin.Bottom;
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
            : Math.Min(anchorBounds.Bottom, root.Height - height - activeOptions.Margin.Bottom);

        AbsoluteLayout.SetLayoutFlags(activePopup, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(activePopup, new Rect(x, y, width, AbsoluteLayout.AutoSize));
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
