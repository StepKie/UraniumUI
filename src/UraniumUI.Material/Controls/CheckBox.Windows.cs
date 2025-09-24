#if WINDOWS
using Microsoft.UI.Xaml.Controls;

namespace UraniumUI.Material.Controls;
public partial class CheckBox
{
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null && args.OldHandler.PlatformView is Panel oldContentPanel)
        {
            oldContentPanel.KeyDown -= PlatformView_KeyDown;
            oldContentPanel.KeyUp -= PlatformView_KeyUp;
        }
        if (args.NewHandler != null && args.NewHandler.PlatformView is Panel newContentPanel)
        {
            newContentPanel.IsTabStop = true;
            newContentPanel.UseSystemFocusVisuals = true;
            newContentPanel.KeyDown += PlatformView_KeyDown;
            newContentPanel.KeyUp += PlatformView_KeyUp;
        }
    }

    private void PlatformView_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (IsActionKey(e.Key))
        {
            VisualStateManager.GoToState(this, "Pressed");
        }
    }

    private void PlatformView_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (IsActionKey(e.Key))
        {
            if (!IsDisabled)
            {
                IsChecked = !IsChecked;
            }
            VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
        }
    }

    private bool IsActionKey(Windows.System.VirtualKey key)
    {
        return key == Windows.System.VirtualKey.Enter || key == Windows.System.VirtualKey.Space;
    }
}
#endif
