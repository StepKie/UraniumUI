#if IOS || MACCATALYST
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace UraniumUI.Blurs;
public class BlurPlatformEffect : PlatformEffect
{
    public BlurEffect VirtualEffect { get; private set; }

    protected UIVisualEffectView blurView;
    private UIColor originalBackgroundColor;
    private Command updateEffectCommand;

    protected override void OnAttached()
    {
        var platformView = this.Control;

        if (Element.Effects.FirstOrDefault(x => x.ResolveId == this.ResolveId) is BlurEffect _effect)
        {
            VirtualEffect = _effect;
            updateEffectCommand = new Command(UpdateEffect);
            _effect.UpdateEffectCommand = updateEffectCommand;
        }

        originalBackgroundColor = Control.BackgroundColor;
        Control.BackgroundColor = UIColor.Clear;

        blurView = new UIVisualEffectView();
        blurView.TranslatesAutoresizingMaskIntoConstraints = false;

        UpdateEffect();

        platformView.InsertSubview(blurView, 0);

        NSLayoutConstraint.ActivateConstraints(new[] {
            blurView.TopAnchor.ConstraintEqualTo(platformView.TopAnchor),
            blurView.LeadingAnchor.ConstraintEqualTo(platformView.LeadingAnchor),
            blurView.HeightAnchor.ConstraintEqualTo(platformView.HeightAnchor),
            blurView.WidthAnchor.ConstraintEqualTo(platformView.WidthAnchor)
        });
    }

    protected override void OnDetached()
    {
        if (VirtualEffect?.UpdateEffectCommand == updateEffectCommand)
        {
            VirtualEffect.UpdateEffectCommand = null;
        }

        blurView?.RemoveFromSuperview();
        blurView?.Dispose();
        blurView = null;

        Control.BackgroundColor = originalBackgroundColor;
        originalBackgroundColor = null;
        updateEffectCommand = null;
        VirtualEffect = null;
    }

    protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
    {
        base.OnElementPropertyChanged(args);
        if (args.PropertyName == View.BackgroundColorProperty.PropertyName && this.Element is View view)
        {
            Control.BackgroundColor = view.BackgroundColor?.WithAlpha(VirtualEffect?.EffectiveAccentOpacity ?? .2f).ToPlatform() ?? UIColor.Clear;
        }
    }

    protected void UpdateEffect()
    {
        if (blurView == null)
        {
            return;
        }

        var accentOpacity = VirtualEffect?.EffectiveAccentOpacity ?? .2f;

        if (VirtualEffect?.AccentColor != null && VirtualEffect.AccentColor.IsNotDefault())
        {
            Control.BackgroundColor = VirtualEffect.AccentColor.WithAlpha(accentOpacity).ToPlatform();
        }
        else
        {
            Control.BackgroundColor = UIColor.Clear;
        }

        blurView.Effect = VirtualEffect?.Mode == BlurMode.Dark ?
            UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark) :
            UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
    }
}

#endif
