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
    private static readonly ConditionalWeakTable<VisualElement, DropdownOverlayHost> hostsByElement = new();

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
            return FindParentHost(anchor)?.Show(anchor, popup, options ?? new DropdownOverlayOptions());
        }

        var host = hosts.GetValue(contentPage, page => new DropdownOverlayHost((ContentPage)page));
        return host.Show(anchor, popup, options ?? new DropdownOverlayOptions());
    }

    internal static void RegisterHostElement(VisualElement element, DropdownOverlayHost host)
    {
        hostsByElement.Remove(element);
        hostsByElement.Add(element, host);
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

    private static DropdownOverlayHost FindParentHost(Element element)
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

internal sealed class DropdownOverlayHost
{
    private readonly ContentPage page;
    private Grid root;
    private AbsoluteLayout overlayLayer;
    private View originalContent;
    private bool ownsRoot;
    private DropdownOverlayRegistration activeRegistration;
    private VisualElement activeAnchor;
    private View activeDismissLayer;
    private View activePopup;
    private DropdownOverlayOptions activeOptions;
    private bool activePopupAnimated;

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
        activePopupAnimated = false;

        AddOverlayLayerToRoot();

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

            var activeOverlayLayer = overlayLayer;

            activeOverlayLayer.Children.Remove(popup);
            activeOverlayLayer.Children.Remove(activeDismissLayer);
            activeOverlayLayer.IsVisible = false;
            activeOverlayLayer.InputTransparent = true;
            root.Children.Remove(activeOverlayLayer);
            root.SizeChanged -= OnRootSizeChanged;
            anchor.SizeChanged -= OnAnchorSizeChanged;

            if (ownsRoot && ReferenceEquals(page.Content, root))
            {
                if (originalContent is not null)
                {
                    root.Children.Remove(originalContent);
                }

                page.Content = originalContent;
            }

            activeRegistration = null;
            activeAnchor = null;
            activeDismissLayer = null;
            activePopup = null;
            activeOptions = null;
            activePopupAnimated = false;
        });

        activeRegistration = registration;
        return registration;
    }

    private void EnsureHost()
    {
        if (root is not null && overlayLayer is not null)
        {
            if (ReferenceEquals(page.Content, root))
            {
                return;
            }

            if (ownsRoot && ReferenceEquals(page.Content, originalContent))
            {
                if (originalContent is not null && !root.Children.Contains(originalContent))
                {
                    root.Children.Insert(0, originalContent);
                }

                RegisterHostElements();
                page.Content = root;
                return;
            }
        }

        if (page.Content is Grid existingRoot)
        {
            root = existingRoot;
            overlayLayer = existingRoot.Children.OfType<AbsoluteLayout>().FirstOrDefault(x => x.StyleId == nameof(DropdownOverlay)) ?? overlayLayer ?? CreateOverlayLayer();
            originalContent = existingRoot;
            ownsRoot = false;
            RegisterHostElements();
            return;
        }

        originalContent = page.Content;
        root = ownsRoot && root is not null ? root : new Grid();
        ownsRoot = true;

        if (originalContent is not null && !root.Children.Contains(originalContent))
        {
            root.Children.Add(originalContent);
        }

        overlayLayer ??= CreateOverlayLayer();

        RegisterHostElements();

        page.Content = root;
    }

    private AbsoluteLayout CreateOverlayLayer()
    {
        return new AbsoluteLayout
        {
            StyleId = nameof(DropdownOverlay),
            BackgroundColor = Colors.Transparent,
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
        DropdownOverlay.RegisterHostElement(root, this);

        if (originalContent is not null)
        {
            DropdownOverlay.RegisterHostElement(originalContent, this);
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

        if (!activePopupAnimated)
        {
            activePopupAnimated = true;
            _ = AnimatePopup(showAbove);
        }
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
