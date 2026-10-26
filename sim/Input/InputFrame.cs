using System;

namespace Glimmerfall.Sim.Input;

[Flags]
public enum InputButtons : byte
{
    None = 0,
    Dash = 1,
    Swap = 2,
    /// <summary>Ассист: следующий живой резервный герой выпрыгивает, бьёт фирменным приёмом и уходит (PRD §5).</summary>
    Assist = 4,
}

/// <summary>
/// Ввод на один шаг сима. Весь ввод игрока — только через него (скилл sim-determinism, правило 5):
/// движение, рывок, свап, выбор в модалке. Значение по умолчанию — «ничего не нажато».
/// </summary>
public struct InputFrame : IEquatable<InputFrame>
{
    /// <summary>Направление движения, −127..127 по осям; длина больше 127 обрезается симом.</summary>
    public sbyte MoveX;
    public sbyte MoveY;
    public InputButtons Buttons;

    /// <summary>Выбор в модалке: 0 — нет выбора, 1..N — вариант с номером (Pick − 1).</summary>
    public byte Pick;

    public static InputFrame Move(int x, int y) => new() { MoveX = ClampAxis(x), MoveY = ClampAxis(y) };

    public static InputFrame Choose(int optionIndex) => new() { Pick = (byte)(optionIndex + 1) };

    public static sbyte ClampAxis(int value) => (sbyte)(value < -127 ? -127 : value > 127 ? 127 : value);

    public readonly bool Equals(InputFrame other) =>
        MoveX == other.MoveX && MoveY == other.MoveY && Buttons == other.Buttons && Pick == other.Pick;

    public override readonly bool Equals(object? obj) => obj is InputFrame other && Equals(other);
    public override readonly int GetHashCode() => (MoveX & 0xFF) | ((MoveY & 0xFF) << 8) | ((int)Buttons << 16) | (Pick << 24);
    public static bool operator ==(InputFrame a, InputFrame b) => a.Equals(b);
    public static bool operator !=(InputFrame a, InputFrame b) => !a.Equals(b);
}
