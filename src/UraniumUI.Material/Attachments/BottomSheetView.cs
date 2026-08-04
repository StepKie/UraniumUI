using InputKit.Shared.Helpers;
using UraniumUI.Extensions;
using UraniumUI.Material.Controls;
using UraniumUI.Pages;
using UraniumUI.Views;

namespace UraniumUI.Material.Attachments;

[ContentProperty(nameof(Body))]
public partial class BottomSheetView : Border, IPageAttachment
{
    public UraniumContentPage AttachedPage { get; protected set; }
    public AttachmentPosition AttachmentPosition => AttachmentPosition.Front;

    public View Body { get; set; }

    public View Header { get; set; }

    public event EventHandler Opened;

    public event EventHandler Closed;

    private TapGestureRecognizer closeGestureRecognizer = new();
    private bool isGeneratedHeader;

    public void OnAttached(UraniumContentPage page)
    {
        Init();

        AttachedPage = page;
        Header.SizeChanged += BottomSheetContent_SizeChanged;
        if (Body != null)
        {
            Body.SizeChanged += BottomSheetContent_SizeChanged;
        }

        AlignBottomSheet(false);
        UpdateTapOutsideGestureRecognizer();
    }

    private void BottomSheetContent_SizeChanged(object sender, EventArgs e)
    {
        AlignBottomSheet(false);
    }

    protected virtual void Init()
    {
        if (Header is null)
        {
            Header = GenerateAnchor();
            isGeneratedHeader = true;
        }

        Padding = 0;
        this.StyleClass = new[] { "BottomSheet" };
        this.VerticalOptions = LayoutOptions.End;
        this.HorizontalOptions = LayoutOptions.Fill;
        this.Content = new VerticalStackLayout()
        {
            Children =
            {
                Header,
                Body
            }
        };

        if (DeviceInfo.Idiom != DeviceIdiom.Desktop)
        {
            var panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += PanGestureRecognizer_PanUpdated;
            Header.GestureRecognizers.Add(panGestureRecognizer);
        }

        if (isGeneratedHeader && Header is StatefulContentView generatedHeader)
        {
            generatedHeader.TappedCommand = new Command(TogglePresented);
            UpdateGeneratedHeaderSemantics();
        }
        else
        {
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => TogglePresented();
            Header.GestureRecognizers.Add(tapGestureRecognizer);
        }

        Header.BackgroundColor ??= this.BackgroundColor;

        closeGestureRecognizer.Tapped += (s, e) => IsPresented = false;
    }

    private void TogglePresented()
    {
        IsPresented = !IsPresented;
    }

    private void OnIsPresentedChanged(bool oldValue, bool newValue)
    {
        if (Header is not null)
        {
            AlignBottomSheet();
        }

        if (oldValue == newValue)
        {
            return;
        }

        if (newValue)
        {
            OnOpened();
        }
        else
        {
            OnClosed();
        }
    }

    protected virtual View GenerateAnchor()
    {
        var anchor = new StatefulContentView
        {
            HorizontalOptions = LayoutOptions.Fill,
            Padding = 10,
            Content = new BoxView
            {
                HeightRequest = 2,
                CornerRadius = 2,
                WidthRequest = 50,
                Color = this.BackgroundColor?.ToSurfaceColor() ?? Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
            }
        };

        return anchor;
    }

    private void UpdateGeneratedHeaderSemantics()
    {
        if (!isGeneratedHeader || Header is null)
        {
            return;
        }

        var options = AccessibilityOptionsProvider.Get();

        SemanticProperties.SetDescription(Header, IsPresented ? options.CollapseBottomSheetDescription : options.ExpandBottomSheetDescription);
        SemanticProperties.SetHint(Header, options.ToggleBottomSheetHint);
    }

    protected virtual void OnOpened()
    {
        UpdateTapOutsideGestureRecognizer();

        Opened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnClosed()
    {
        UpdateTapOutsideGestureRecognizer();

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTapOutsideGestureRecognizer()
    {
        var gestureRecognizers = AttachedPage?.ContentFrame?.GestureRecognizers;
        if (gestureRecognizers is null)
        {
            return;
        }

        if (IsPresented && CloseOnTapOutside)
        {
            if (!gestureRecognizers.Contains(closeGestureRecognizer))
            {
                gestureRecognizers.Add(closeGestureRecognizer);
            }
        }
        else
        {
            gestureRecognizers.Remove(closeGestureRecognizer);
        }
    }

    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                var isApple = DeviceInfo.Current.Platform == DevicePlatform.iOS || DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;

                var y = TranslationY + (isApple ? e.TotalY * .05 : e.TotalY);

                this.TranslationY = y.Clamp(-50, this.Height);

                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (this.TranslationY < this.Height * .5)
                {
                    IsPresented = true;
                }
                else
                {
                    IsPresented = false;
                }
                AlignBottomSheet();
                break;
        }
    }

    private void AlignBottomSheet(bool animate = true)
    {
        double y = this.Height - Header.Height;
        if (IsPresented)
        {
            y = 0;
        }

        if (animate)
        {
            this.TranslateToSafely(this.X, y, 50);

        }
        else
        {
            this.TranslationY = y;
        }

        UpdateDisabledStateOfPage();
        UpdateGeneratedHeaderSemantics();
    }

    protected void UpdateDisabledStateOfPage()
    {
        if (AttachedPage?.Body != null && DisablePageWhenOpened)
        {
            AttachedPage.Body.InputTransparent = IsPresented;

            AttachedPage.Body.FadeToSafely(IsPresented ? .5 : 1);
        }
    }
}
