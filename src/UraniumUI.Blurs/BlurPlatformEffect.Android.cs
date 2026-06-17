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
    private const float DefaultBlurRadius = 24f;

    public Context Context => Control?.Context;

    private BlurView _blurView;
    private GradientDrawable _mainDrawable;
    private Drawable _originalBackground;
    private ViewGroup _blurRoot;
    private Command _updateEffectCommand;
    private Command _invalidateEffectCommand;
    private bool _nativeRenderEffectApplied;
    private AndroidBlurCaptureMode _blurCaptureMode;
    private int _blurCaptureFps;
    private float _blurCaptureDownsampleFactor;

    public BlurEffect VirtualEffect { get; private set; }

    protected override void OnAttached()
    {
        if (Element.Effects.FirstOrDefault(x => x.ResolveId == this.ResolveId) is BlurEffect blurEffect)
        {
            VirtualEffect = blurEffect;
            _updateEffectCommand = new Command(UpdateEffect);
            _invalidateEffectCommand = new Command(InvalidateBlur);
            blurEffect.UpdateEffectCommand = _updateEffectCommand;
            blurEffect.InvalidateEffectCommand = _invalidateEffectCommand;
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
        ClearNativeRenderEffect();

        if (Element is Microsoft.Maui.Controls.View view)
        {
            view.SizeChanged -= BlurPlatformEffect_SizeChanged;
            view.ParentChanged -= View_ParentChanged;
        }

        if (VirtualEffect?.UpdateEffectCommand == _updateEffectCommand)
        {
            VirtualEffect.UpdateEffectCommand = null;
        }

        if (VirtualEffect?.InvalidateEffectCommand == _invalidateEffectCommand)
        {
            VirtualEffect.InvalidateEffectCommand = null;
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
        _invalidateEffectCommand = null;
        VirtualEffect = null;
    }

    private void BlurPlatformEffect_SizeChanged(object sender, EventArgs e)
    {
        AlignBlurView();

        if (_blurView != null)
        {
            UpdateEffect();
        }
    }

    private void View_ParentChanged(object sender, EventArgs e)
    {
        UpdateEffect();
    }

    protected void UpdateEffect()
    {
        if (Control == null || Context == null)
        {
            ClearNativeRenderEffect();
            ReleaseBlurView();
            return;
        }

        EnsureBackgroundDrawable();

        switch (ResolveAndroidStrategy())
        {
            case AndroidBlurStrategy.RealtimeCapture:
                ApplyCaptureStrategy(AndroidBlurCaptureMode.Realtime);
                break;
            case AndroidBlurStrategy.StaticCapture:
                ApplyCaptureStrategy(AndroidBlurCaptureMode.Static);
                break;
            case AndroidBlurStrategy.RenderEffect:
                ApplyRenderEffectStrategy();
                break;
            default:
                ApplyMaterialStrategy();
                break;
        }
    }

    private void ApplyMaterialStrategy()
    {
        ClearNativeRenderEffect();
        ReleaseBlurView();
        _mainDrawable.SetColor(GetColor());
    }

    private void ApplyRenderEffectStrategy()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            ApplyMaterialStrategy();
            return;
        }

        ReleaseBlurView();
        _mainDrawable.SetColor(GetColor());

        Control.SetRenderEffect(Android.Graphics.RenderEffect.CreateBlurEffect(
            DefaultBlurRadius,
            DefaultBlurRadius,
            Android.Graphics.Shader.TileMode.Clamp));
        _nativeRenderEffectApplied = true;
    }

    private void ApplyCaptureStrategy(AndroidBlurCaptureMode captureMode)
    {
        ClearNativeRenderEffect();

        if (Control is not ViewGroup viewGroup)
        {
            ApplyMaterialStrategy();
            return;
        }

        _mainDrawable.SetColor(Colors.Transparent.ToPlatform());

        if (_blurView == null)
        {
            _blurView = new BlurView(Context);
            _blurView.SetOverlayColor(Color.Transparent);
        }

        var captureFps = VirtualEffect?.EffectiveAndroidRealtimeCaptureFps ?? BlurViewDefaults.REALTIME_CAPTURE_FPS;
        var downsampleFactor = VirtualEffect?.EffectiveAndroidCaptureDownsampleFactor ?? BlurViewDefaults.CAPTURE_SCALE_FACTOR;

        _blurView.CaptureMode = captureMode;
        _blurView.RealtimeCaptureFps = captureFps;
        _blurView.CaptureDownsampleFactor = downsampleFactor;

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
        var root = GetCaptureRoot(viewGroup, decorView);
        if (root == null)
        {
            _blurView.Release();
            _blurRoot = null;
            return;
        }

        if (_blurRoot != root || CaptureOptionsChanged(captureMode, captureFps, downsampleFactor))
        {
            _blurRoot = root;
            _blurCaptureMode = captureMode;
            _blurCaptureFps = captureFps;
            _blurCaptureDownsampleFactor = downsampleFactor;
            _blurView
               .SetupWith(root)
               .SetFrameClearDrawable(root.Background ?? decorView?.Background)
               .SetBlurRadius(DefaultBlurRadius);
        }
    }

    private bool CaptureOptionsChanged(AndroidBlurCaptureMode captureMode, int captureFps, float downsampleFactor)
    {
        return _blurCaptureMode != captureMode
            || _blurCaptureFps != captureFps
            || Math.Abs(_blurCaptureDownsampleFactor - downsampleFactor) > 0.001f;
    }

    private ViewGroup GetCaptureRoot(ViewGroup viewGroup, Android.Views.View decorView)
    {
        return GetClosestCaptureRoot(viewGroup)
            ?? decorView?.FindViewById(global::Android.Resource.Id.Content) as ViewGroup;
    }

    private ViewGroup GetClosestCaptureRoot(ViewGroup viewGroup)
    {
        var parent = viewGroup.Parent as ViewGroup;

        while (parent != null)
        {
            if (parent.Width > 0 && parent.Height > 0)
            {
                return parent;
            }

            parent = parent.Parent as ViewGroup;
        }

        return null;
    }

    private AndroidBlurStrategy ResolveAndroidStrategy()
    {
        var strategy = VirtualEffect?.AndroidStrategy ?? AndroidBlurStrategy.Default;
        if (strategy != AndroidBlurStrategy.Default)
        {
            return strategy;
        }

        return AndroidBlurStrategy.Material;
    }

    private void EnsureBackgroundDrawable()
    {
        if (_mainDrawable != null)
        {
            return;
        }

        _originalBackground = Control.Background;
        _mainDrawable = new GradientDrawable();
        _mainDrawable.SetColor(Colors.Transparent.ToPlatform());
        Control.Background = _mainDrawable;
    }

    private void ClearNativeRenderEffect()
    {
        if (!_nativeRenderEffectApplied || Control == null || !OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            _nativeRenderEffectApplied = false;
            return;
        }

        Control.SetRenderEffect(null);
        _nativeRenderEffectApplied = false;
    }

    private void InvalidateBlur()
    {
        _blurView?.InvalidateBlur();
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
