#if ANDROID
using Android.Views;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UraniumUI.Views;
using static Microsoft.Maui.Controls.VisualStateManager;

namespace UraniumUI.Handlers;
public partial class StatefulContentViewHandler
{
    private bool suppressActionKeyUp;

    protected override ContentViewGroup CreatePlatformView()
    {
        var platformView = base.CreatePlatformView();

        platformView.Touch += OnTouch;
        platformView.Hover += NativeView_Hover;
        platformView.Click += PlatformView_Click;
        platformView.LongClick += PlatformView_LongClick;
        platformView.KeyPress += PlatformView_KeyPress;
        platformView.Focusable = StatefulView.IsFocusable;
        platformView.FocusableInTouchMode = StatefulView.IsFocusable;

        return platformView;
    }

    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        platformView.Touch -= OnTouch;
        platformView.Hover -= NativeView_Hover;
        platformView.Click -= PlatformView_Click;
        platformView.LongClick -= PlatformView_LongClick;
        platformView.KeyPress -= PlatformView_KeyPress;
        base.DisconnectHandler(platformView);
    }

    private void NativeView_Hover(object sender, Android.Views.View.HoverEventArgs e)
    {
        if (e.Event.Action == MotionEventActions.HoverEnter)
        {
            GoToState(StatefulView, CommonStates.PointerOver);
            StatefulView.InvokeHovered();
            ExecuteCommandIfCan(StatefulView.HoverCommand);
            return;
        }

        if (e.Event.Action == MotionEventActions.HoverExit)
        {
            GoToState(StatefulView, CommonStates.Normal);
            StatefulView.InvokeHoverExited();
            ExecuteCommandIfCan(StatefulView.HoverExitCommand);
        }
    }

    private void OnTouch(object sender, Android.Views.View.TouchEventArgs e)
    {
        if (e.Event.Action == MotionEventActions.Down)
        {
            GoToState(StatefulView, "Pressed");
            StatefulView.InvokePressed();
            ExecuteCommandIfCan(StatefulView.PressedCommand);
            e.Handled = false;
        }
        else if (e.Event.Action == MotionEventActions.Up)
        {
            GoToState(StatefulView, CommonStates.Normal);
            e.Handled = false;
        }
    }

    private void PlatformView_Click(object sender, EventArgs e)
    {
        GoToState(StatefulView, CommonStates.Normal);
        StatefulView.InvokeTapped();
        ExecuteCommandIfCan(StatefulView.TappedCommand);
    }

    private void PlatformView_LongClick(object sender, Android.Views.View.LongClickEventArgs e)
    {
        StatefulView.InvokeLongPressed();
        ExecuteCommandIfCan(StatefulView.LongPressCommand);
    }

    private void PlatformView_KeyPress(object sender, Android.Views.View.KeyEventArgs e)
    {
        var key = ToStatefulKey(e.KeyCode);

        if (key is null)
        {
            return;
        }

        if (e.Event.Action == KeyEventActions.Down && StatefulView.SendKeyDown(key.Value))
        {
            e.Handled = true;
            suppressActionKeyUp = IsActionKey(e.KeyCode);
            return;
        }

        if (e.Event.Action == KeyEventActions.Up && IsActionKey(e.KeyCode) && suppressActionKeyUp)
        {
            suppressActionKeyUp = false;
            e.Handled = true;
        }
    }

    private static StatefulContentViewKey? ToStatefulKey(Keycode key)
    {
        return key switch
        {
            Keycode.Enter => StatefulContentViewKey.Enter,
            Keycode.Space => StatefulContentViewKey.Space,
            Keycode.Escape => StatefulContentViewKey.Escape,
            Keycode.Back => StatefulContentViewKey.Escape,
            Keycode.DpadDown => StatefulContentViewKey.ArrowDown,
            Keycode.DpadUp => StatefulContentViewKey.ArrowUp,
            Keycode.MoveHome => StatefulContentViewKey.Home,
            Keycode.MoveEnd => StatefulContentViewKey.End,
            _ => null,
        };
    }

    private static bool IsActionKey(Keycode key)
    {
        return key == Keycode.Enter || key == Keycode.Space;
    }

    public static void MapIsFocusable(StatefulContentViewHandler handler, StatefulContentView view)
    {
        handler.PlatformView.Focusable = view.IsFocusable;
        handler.PlatformView.FocusableInTouchMode = view.IsFocusable;
    }
}
#endif
