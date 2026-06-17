#if WINDOWS

using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Platform;
using WinBrush = Microsoft.UI.Xaml.Media.Brush;

namespace UraniumUI.Blurs;
public class BlurPlatformEffect : PlatformEffect
{
    public BlurEffect VirtualEffect { get; private set; }
    private Command updateEffectCommand;
    private WinBrush originalControlBackground;
    private WinBrush originalPanelBackground;
    private WinBrush originalBorderBackground;
    private bool hasOriginalControlBackground;
    private bool hasOriginalPanelBackground;
    private bool hasOriginalBorderBackground;

    protected override void OnAttached()
    {
        if (Element.Effects.FirstOrDefault(x => x.ResolveId == this.ResolveId) is BlurEffect blurEffect)
        {
            VirtualEffect = blurEffect;
            updateEffectCommand = new Command(UpdateEffect);
            blurEffect.UpdateEffectCommand = updateEffectCommand;
        }

        UpdateEffect();
    }

    protected void UpdateEffect()
    {
        if (Control is Control control)
        {
            if (!hasOriginalControlBackground)
            {
                originalControlBackground = control.Background;
                hasOriginalControlBackground = true;
            }

            control.Background = GetBrush();
        }

        if (Control is Panel panel)
        {
            if (!hasOriginalPanelBackground)
            {
                originalPanelBackground = panel.Background;
                hasOriginalPanelBackground = true;
            }

            panel.Background = GetBrush();
        }

        if (Control is Microsoft.UI.Xaml.Controls.Border border)
        {
            if (!hasOriginalBorderBackground)
            {
                originalBorderBackground = border.Background;
                hasOriginalBorderBackground = true;
            }

            border.Background = GetBrush();
        }
    }

    protected AcrylicBrush GetBrush()
    {
        var accentOpacity = VirtualEffect?.EffectiveAccentOpacity ?? .2f;

        if (VirtualEffect?.AccentColor != null && VirtualEffect.AccentColor.IsNotDefault())
        {
            return new AcrylicBrush
            {
                TintColor = VirtualEffect.AccentColor.ToWindowsColor(),
                TintOpacity =  accentOpacity,
                TintLuminosityOpacity = .4
            };
        }

        return new AcrylicBrush
        {
            TintColor = VirtualEffect?.Mode == BlurMode.Dark ? Colors.Black.ToWindowsColor() : Colors.DimGray.ToWindowsColor(),
            TintOpacity = accentOpacity,
            TintLuminosityOpacity = .4
        };
    }

    protected override void OnDetached()
    {
        if (VirtualEffect?.UpdateEffectCommand == updateEffectCommand)
        {
            VirtualEffect.UpdateEffectCommand = null;
        }

        if (Control is Control control)
        {
            control.Background = hasOriginalControlBackground ? originalControlBackground : null;
        }

        if (Control is Panel panel)
        {
            panel.Background = hasOriginalPanelBackground ? originalPanelBackground : null;
        }

        if (Control is Microsoft.UI.Xaml.Controls.Border border)
        {
            border.Background = hasOriginalBorderBackground ? originalBorderBackground : null;
        }

        updateEffectCommand = null;
        originalControlBackground = null;
        originalPanelBackground = null;
        originalBorderBackground = null;
        hasOriginalControlBackground = false;
        hasOriginalPanelBackground = false;
        hasOriginalBorderBackground = false;
        VirtualEffect = null;
    }
}

#endif
