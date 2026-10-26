using Glimmerfall.Sim;
using Glimmerfall.Sim.Input;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Клавиатура и геймпад → <see cref="InputFrame"/>. Всё, что делает игрок, идёт в сим только так — тот же путь, что у
/// реплея и бота. Тач-стик (PRD §11) встанет сюда же в мобильном спайке.
/// </summary>
public static class PlayerInput
{
    private const float Deadzone = 0.2f;

    public static InputFrame Read(World world)
    {
        if (world.Modal != ModalKind.None)
        {
            for (int k = 0; k < world.ModalOptionCount; k++)
            {
                if (Input.IsKeyPressed(Key.Key1 + k) || (k == 0 && Input.IsJoyButtonPressed(0, JoyButton.A)))
                {
                    return InputFrame.Choose(k);
                }
            }
            return default;
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
        var buttons = InputButtons.None;
        if (Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Shift) || Input.IsJoyButtonPressed(0, JoyButton.A))
        {
            buttons |= InputButtons.Dash;
        }
        if (Input.IsKeyPressed(Key.Q) || Input.IsKeyPressed(Key.E) || Input.IsJoyButtonPressed(0, JoyButton.X))
        {
            buttons |= InputButtons.Swap;
        }
        InputFrame frame = InputFrame.Move((int)(x * 127f), (int)(y * 127f));
        frame.Buttons = buttons;
        return frame;
    }

    private static float Axis(Key primary, Key secondary) => Input.IsKeyPressed(primary) || Input.IsKeyPressed(secondary) ? 1f : 0f;
}
