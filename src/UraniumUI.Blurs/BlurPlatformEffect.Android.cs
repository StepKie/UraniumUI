#if ANDROID
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Color = Android.Graphics.Color;

namespace UraniumUI.Blurs;
public class BlurPlatformEffect : PlatformEffect
{
    public Context Context => Control?.Context;

    private BlurView _blurView;
    private GradientDrawable _mainDrawable;
    private Drawable _originalBackground;
    private ViewGroup _blurRoot;
    private Command _updateEffectCommand;

    public BlurEffect VirtualEffect { get; private set; }

    protected override void OnAttached()
    {
        if (Element.Effects.FirstOrDefault(x => x.ResolveId == this.ResolveId) is BlurEffect blurEffect)
        {
            VirtualEffect = blurEffect;
            _updateEffectCommand = new Command(UpdateEffect);
            blurEffect.UpdateEffectCommand = _updateEffectCommand;
        }

        if (Element is Microsoft.Maui.Controls.View view)
        {
            view.SizeChanged += BlurPlatformEffect_SizeChanged;
            view.ParentChanged += View_ParentChanged;
        }

        UpdateEffect();
    }

    protected override void OnDetached()
    {
        if (Element is Microsoft.Maui.Controls.View view)
        {
            view.SizeChanged -= BlurPlatformEffect_SizeChanged;
            view.ParentChanged -= View_ParentChanged;
        }

        if (VirtualEffect?.UpdateEffectCommand == _updateEffectCommand)
        {
            VirtualEffect.UpdateEffectCommand = null;
        }

        ReleaseBlurView();

        if (_mainDrawable != null)
        {
            Control.Background = _originalBackground;
            _mainDrawable.Dispose();
            _mainDrawable = null;
            _originalBackground = null;
        }

        _updateEffectCommand = null;
        VirtualEffect = null;
    }

    private void BlurPlatformEffect_SizeChanged(object sender, EventArgs e)
    {
        AlignBlurView();
    }

    private void View_ParentChanged(object sender, EventArgs e)
    {
        UpdateEffect();
    }

    protected void UpdateEffect()
    {
        if (Control is ViewGroup viewGroup && Context != null)
        {
            if (_mainDrawable == null)
            {
                _originalBackground = Control.Background;
                _mainDrawable = new GradientDrawable();
                _mainDrawable.SetColor(Colors.Transparent.ToPlatform());
                Control.Background = _mainDrawable;
            }

            if (_blurView == null)
            {
                _blurView = new BlurView(Context);
                _blurView.SetOverlayColor(Color.Transparent);
            }

            if (_blurView.Parent != viewGroup)
            {
                if (_blurView.Parent is ViewGroup previousParent)
                {
                    previousParent.RemoveView(_blurView);
                }

                viewGroup.AddView(_blurView, 0, new FrameLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent,
                    GravityFlags.NoGravity));
            }

            AlignBlurView();

            _blurView.SetBackgroundColor(GetColor());

            var decorView = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView;
            var root = decorView?.FindViewById(global::Android.Resource.Id.Content) as ViewGroup;
            if (root == null)
            {
                _blurView.Release();
                _blurRoot = null;
                return;
            }

            if (_blurRoot != root)
            {
                _blurRoot = root;
                _blurView
                   .SetupWith(root)
                   .SetFrameClearDrawable(decorView.Background)
                   .SetBlurRadius(24f);
            }
        }
        else
        {
            ReleaseBlurView();
        }
    }

    protected Android.Graphics.Color GetColor()
    {
        var accentOpacity = VirtualEffect?.EffectiveAccentOpacity ?? .2f;

        if (VirtualEffect?.AccentColor != null && VirtualEffect.AccentColor.IsNotDefault())
        {
            return VirtualEffect.AccentColor.WithAlpha(accentOpacity).ToPlatform();
        }

        return VirtualEffect?.Mode == BlurMode.Dark
            ? Colors.Black.WithAlpha(accentOpacity).ToPlatform()
            : Colors.White.WithAlpha(accentOpacity).ToPlatform();
    }

    private void ReleaseBlurView()
    {
        if (_blurView == null)
        {
            _blurRoot = null;
            return;
        }

        _blurView.Release();

        if (_blurView.Parent is ViewGroup parent)
        {
            parent.RemoveView(_blurView);
        }

        _blurView.Dispose();
        _blurView = null;
        _blurRoot = null;
    }

    private void AlignBlurView()
    {
        var PlatformView = Control;

        if (PlatformView == null || _blurView == null)
        {
            return;
        }

        int width = PlatformView.MeasuredWidth;
        int height = PlatformView.MeasuredHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        _blurView.Measure(
            Android.Views.View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
            Android.Views.View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
        _blurView.Layout(0, 0, width, height);
        _blurView.UpdateBlurViewSize();
    }
}

#endif
