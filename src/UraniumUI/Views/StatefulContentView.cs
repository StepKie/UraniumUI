using System.Windows.Input;
using UraniumUI.Handlers;

namespace UraniumUI.Views;

internal enum StatefulContentViewKey
{
    Enter,
    Space,
    Escape,
    ArrowDown,
    ArrowUp,
    Home,
    End,
}

internal sealed class StatefulContentViewKeyEventArgs : EventArgs
{
    public StatefulContentViewKeyEventArgs(StatefulContentViewKey key)
    {
        Key = key;
    }

    public StatefulContentViewKey Key { get; }

    public bool Handled { get; set; }
}

public class StatefulContentView : ContentView, IStatefulView
{
    public event EventHandler<EventArgs> Pressed;
    public event EventHandler<EventArgs> LongPressed;
    public event EventHandler<EventArgs> Hovered;
    public event EventHandler<EventArgs> HoverExited;
    public event EventHandler<EventArgs> Tapped;

    internal event EventHandler<StatefulContentViewKeyEventArgs> KeyDown;

    internal void InvokePressed() => Pressed?.Invoke(this, EventArgs.Empty);

    internal void InvokeLongPressed() => LongPressed?.Invoke(this, EventArgs.Empty);

    internal void InvokeHovered() => Hovered?.Invoke(this, EventArgs.Empty);

    internal void InvokeHoverExited() => HoverExited?.Invoke(this, EventArgs.Empty);

    internal void InvokeTapped() => Tapped?.Invoke(this, EventArgs.Empty);

    internal bool SendKeyDown(StatefulContentViewKey key)
    {
        var args = new StatefulContentViewKeyEventArgs(key);
        KeyDown?.Invoke(this, args);
        return args.Handled;
    }

    public ICommand PressedCommand { get => (ICommand)GetValue(PressedCommandProperty); set => SetValue(PressedCommandProperty, value); }

    public static BindableProperty PressedCommandProperty = BindableProperty.Create(nameof(PressedCommand), typeof(ICommand), typeof(StatefulContentView));

    public ICommand HoverCommand { get => (ICommand)GetValue(HoverCommandProperty); set => SetValue(HoverCommandProperty, value); }

    public static BindableProperty HoverCommandProperty = BindableProperty.Create(nameof(HoverCommand), typeof(ICommand), typeof(StatefulContentView));

    public ICommand HoverExitCommand { get => (ICommand)GetValue(HoverExitCommandProperty); set => SetValue(HoverExitCommandProperty, value); }

    public static BindableProperty HoverExitCommandProperty = BindableProperty.Create(nameof(HoverExitCommand), typeof(ICommand), typeof(StatefulContentView));

    public ICommand LongPressCommand { get => (ICommand)GetValue(LongPressCommandProperty); set => SetValue(LongPressCommandProperty, value); }

    public static BindableProperty LongPressCommandProperty = BindableProperty.Create(nameof(LongPressCommand), typeof(ICommand), typeof(StatefulContentView));

    public ICommand TappedCommand { get => (ICommand)GetValue(TappedCommandProperty); set => SetValue(TappedCommandProperty, value); }

    public static BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(StatefulContentView));

    public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(StatefulContentView));

    public bool IsFocusable { get => (bool)GetValue(IsFocusableProperty); set => SetValue(IsFocusableProperty, value); }

    public static BindableProperty IsFocusableProperty = BindableProperty.Create(nameof(IsFocusable), typeof(bool), typeof(StatefulContentView), true);
}
