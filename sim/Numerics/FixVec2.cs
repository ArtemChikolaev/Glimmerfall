using System;

namespace Glimmerfall.Sim.Numerics;

/// <summary>Двумерный вектор в <see cref="Fix"/>. Длина — через целочисленный корень, без float.</summary>
public readonly struct FixVec2 : IEquatable<FixVec2>
{
    public readonly Fix X;
    public readonly Fix Y;

    public FixVec2(Fix x, Fix y)
    {
        X = x;
        Y = y;
    }

    public static readonly FixVec2 Zero = new(Fix.Zero, Fix.Zero);

    public static FixVec2 FromInts(int x, int y) => new(Fix.FromInt(x), Fix.FromInt(y));

    public static FixVec2 operator +(FixVec2 a, FixVec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static FixVec2 operator -(FixVec2 a, FixVec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static FixVec2 operator -(FixVec2 a) => new(-a.X, -a.Y);
    public static FixVec2 operator *(FixVec2 a, Fix k) => new(a.X * k, a.Y * k);
    public static FixVec2 operator /(FixVec2 a, Fix k) => new(a.X / k, a.Y / k);
    public static bool operator ==(FixVec2 a, FixVec2 b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(FixVec2 a, FixVec2 b) => !(a == b);

    public static Fix Dot(FixVec2 a, FixVec2 b) => a.X * b.X + a.Y * b.Y;

    /// <summary>Квадрат длины в сырых единицах Q32.32 (x²+y² по Raw): без сдвига, чтобы не терять точность у малых векторов.
    /// Безопасно при |X|, |Y| &lt; 46 000 единиц.</summary>
    public long LengthSquaredRaw => X.Raw * X.Raw + Y.Raw * Y.Raw;

    public Fix Length => Fix.FromRaw((long)FixMath.ISqrt((ulong)LengthSquaredRaw));

    /// <summary>Единичный вектор; нулевой остаётся нулевым.</summary>
    public FixVec2 Normalized()
    {
        Fix length = Length;
        return length.Raw == 0 ? Zero : new FixVec2(X / length, Y / length);
    }

    /// <summary>Поворот на угол в единицах <see cref="FixMath.FullTurn"/>.</summary>
    public FixVec2 Rotated(int angle)
    {
        Fix c = FixMath.Cos(angle);
        Fix s = FixMath.Sin(angle);
        return new FixVec2(X * c - Y * s, X * s + Y * c);
    }

    public bool Equals(FixVec2 other) => this == other;
    public override bool Equals(object? obj) => obj is FixVec2 other && this == other;
    public override int GetHashCode() => X.GetHashCode() * 397 ^ Y.GetHashCode();
    public override string ToString() => "(" + X + ", " + Y + ")";
}
