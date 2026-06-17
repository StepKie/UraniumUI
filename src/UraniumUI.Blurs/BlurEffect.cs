using System.Windows.Input;

namespace UraniumUI.Blurs;
public class BlurEffect : RoutingEffect
{
    private BlurMode mode;
    private Color accentColor;
    private float accentOpacity = .2f;
    private AndroidBlurStrategy androidStrategy;
    private int androidRealtimeCaptureFps = 15;
    private float androidCaptureDownsampleFactor = 8f;

    public BlurMode Mode { get => mode; set { mode = value; UpdateEffectCommand?.Execute(this); } }

    public Color AccentColor { get => accentColor; set { accentColor = value; UpdateEffectCommand?.Execute(this); } }

    public float AccentOpacity { get => accentOpacity; set { accentOpacity = value; UpdateEffectCommand?.Execute(this); } }

    public AndroidBlurStrategy AndroidStrategy { get => androidStrategy; set { androidStrategy = value; UpdateEffectCommand?.Execute(this); } }

    public int AndroidRealtimeCaptureFps { get => androidRealtimeCaptureFps; set { androidRealtimeCaptureFps = value; UpdateEffectCommand?.Execute(this); } }

    public float AndroidCaptureDownsampleFactor { get => androidCaptureDownsampleFactor; set { androidCaptureDownsampleFactor = value; UpdateEffectCommand?.Execute(this); } }

    internal float EffectiveAccentOpacity => Math.Clamp(AccentOpacity, 0f, 1f);

    internal int EffectiveAndroidRealtimeCaptureFps => Math.Clamp(AndroidRealtimeCaptureFps, 1, 60);

    internal float EffectiveAndroidCaptureDownsampleFactor => Math.Clamp(AndroidCaptureDownsampleFactor, 1f, 32f);

    internal ICommand UpdateEffectCommand { get; set; }

    internal ICommand InvalidateEffectCommand { get; set; }

    public void InvalidateAndroidBlur()
    {
        InvalidateEffectCommand?.Execute(this);
    }

    public BlurEffect()
    {
        mode = Application.Current?.RequestedTheme == AppTheme.Dark ? BlurMode.Dark : BlurMode.Light;
    }
}

public enum BlurMode
{
    Light,
    Dark,
}

public enum AndroidBlurStrategy
{
    Default,
    Material,
    RenderEffect,
    RealtimeCapture,
    StaticCapture,
}
