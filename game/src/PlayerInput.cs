using Glimmerfall.Sim;
using Glimmerfall.Sim.Input;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Клавиатура, геймпад и тач → <see cref="InputFrame"/>. Всё, что делает игрок, идёт в сим только так — тот же путь,
/// что у реплея и бота. Выбор в модалке приходит из карточек интерфейса (<paramref name="pick"/>).
/// </summary>
public static class PlayerInput
{
    private const float Deadzone = 0.2f;

    public static InputFrame Read(World world, TouchControls touch, int pick)
    {
        if (world.Modal != ModalKind.None)
        {
            return pick >= 0 ? InputFrame.Choose(pick) : default;
        }
        float x = Axis(Key.D, Key.Right) - Axis(Key.A, Key.Left);
        float y = Axis(Key.S, Key.Down) - Axis(Key.W, Key.Up);
        float sx = Input.GetJoyAxis(0, JoyAxis.LeftX);
        float sy = Input.GetJoyAxis(0, JoyAxis.LeftY);
        if (sx * sx + sy * sy > Deadzone * Deadzone)
        {
            x = sx;
            y = sy;
        }
        Vector2 stick = touch.Stick;
        if (stick != Vector2.Zero)
        {
            x = stick.X;
            y = stick.Y;
        }
        var buttons = InputButtons.None;
        if (Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Shift) || Input.IsJoyButtonPressed(0, JoyButton.A) || touch.DashHeld)
        {
            buttons |= InputButtons.Dash;
        }
        if (Input.IsKeyPressed(Key.Q) || Input.IsJoyButtonPressed(0, JoyButton.X) || touch.SwapHeld)
        {
            buttons |= InputButtons.Swap;
        }
        if (Input.IsKeyPressed(Key.E) || Input.IsJoyButtonPressed(0, JoyButton.Y) || touch.AssistHeld)
        {
            buttons |= InputButtons.Assist;
        }
        InputFrame frame = InputFrame.Move((int)(x * 127f), (int)(y * 127f));
        frame.Buttons = buttons;
        return frame;
    }

    private static float Axis(Key primary, Key secondary) => Input.IsKeyPressed(primary) || Input.IsKeyPressed(secondary) ? 1f : 0f;
}
