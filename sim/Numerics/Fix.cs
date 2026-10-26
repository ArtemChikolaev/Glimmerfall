using System;
using System.Globalization;

namespace Glimmerfall.Sim.Numerics;

/// <summary>
/// Фиксированная точка Q48.16 в <see cref="long"/>: 16 дробных бит, шаг 1/65536.
/// Произведение считается в 64 битах без 128-битного промежуточного: |a·b| в реальных единицах должно быть
/// меньше 2^31 (≈ 2,1·10^9), иначе переполнение. Оно в C# без checked детерминировано, но это ошибка логики.
/// Координаты мира — пиксели базовой сетки, скорости — пиксели за тик.
/// </summary>
public readonly struct Fix : IEquatable<Fix>, IComparable<Fix>
{
    public const int FractionBits = 16;
    public const long OneRaw = 1L << FractionBits;

    public readonly long Raw;

    private Fix(long raw) => Raw = raw;

    public static readonly Fix Zero = new(0);
    public static readonly Fix One = new(OneRaw);
    public static readonly Fix Half = new(OneRaw / 2);

    public static Fix FromRaw(long raw) => new(raw);
    public static Fix FromInt(int value) => new((long)value << FractionBits);

    /// <summary>numerator/denominator с усечением к нулю: Ratio(3, 2) = 1,5.</summary>
    public static Fix Ratio(long numerator, long denominator) => new((numerator << FractionBits) / denominator);

    /// <summary>Тысячные доли: Milli(1500) = 1,5. Константы контента задаются так, без float.</summary>
    public static Fix Milli(long thousandths) => Ratio(thousandths, 1000);

    /// <summary>Величина в единицах в секунду → в единицах за тик (60 Гц).</summary>
    public static Fix PerSecond(long unitsPerSecond) => Ratio(unitsPerSecond, SimConstants.TicksPerSecond);

    public int FloorToInt() => (int)(Raw >> FractionBits);
    public int CeilToInt() => (int)((Raw + OneRaw - 1) >> FractionBits);
    public int RoundToInt() => (int)((Raw + (OneRaw >> 1)) >> FractionBits);

    public static Fix operator +(Fix a, Fix b) => new(a.Raw + b.Raw);
    public static Fix operator -(Fix a, Fix b) => new(a.Raw - b.Raw);
    public static Fix operator -(Fix a) => new(-a.Raw);
    public static Fix operator *(Fix a, Fix b) => new((a.Raw * b.Raw) >> FractionBits);
    public static Fix operator /(Fix a, Fix b) => new((a.Raw << FractionBits) / b.Raw);
    public static Fix operator *(Fix a, int b) => new(a.Raw * b);
    public static Fix operator *(int a, Fix b) => new(a * b.Raw);
    public static Fix operator /(Fix a, int b) => new(a.Raw / b);

    public static bool operator ==(Fix a, Fix b) => a.Raw == b.Raw;
    public static bool operator !=(Fix a, Fix b) => a.Raw != b.Raw;
    public static bool operator <(Fix a, Fix b) => a.Raw < b.Raw;
    public static bool operator >(Fix a, Fix b) => a.Raw > b.Raw;
    public static bool operator <=(Fix a, Fix b) => a.Raw <= b.Raw;
    public static bool operator >=(Fix a, Fix b) => a.Raw >= b.Raw;

    public static Fix Abs(Fix a) => a.Raw < 0 ? new Fix(-a.Raw) : a;
    public static Fix Min(Fix a, Fix b) => a.Raw < b.Raw ? a : b;
    public static Fix Max(Fix a, Fix b) => a.Raw > b.Raw ? a : b;
    public static Fix Clamp(Fix value, Fix min, Fix max) => value.Raw < min.Raw ? min : value.Raw > max.Raw ? max : value;

    /// <summary>Корень с округлением вниз; для отрицательных — 0. Точен до шага 1/65536 при Raw &lt; 2^47.</summary>
    public static Fix Sqrt(Fix a) => a.Raw <= 0 ? Zero : new Fix((long)FixMath.ISqrt((ulong)a.Raw << FractionBits));

    /// <summary>Линейная интерполяция a + (b − a)·t.</summary>
    public static Fix Lerp(Fix a, Fix b, Fix t) => a + (b - a) * t;

    public bool Equals(Fix other) => Raw == other.Raw;
    public override bool Equals(object? obj) => obj is Fix other && Raw == other.Raw;
    public override int GetHashCode() => Raw.GetHashCode();
    public int CompareTo(Fix other) => Raw.CompareTo(other.Raw);

    /// <summary>Десятичная запись с 4 знаками, только целочисленной арифметикой (для логов и отчётов).</summary>
    public override string ToString()
    {
        bool negative = Raw < 0;
        ulong abs = negative ? (ulong)(-(Raw + 1)) + 1 : (ulong)Raw;
        ulong whole = abs >> FractionBits;
        ulong frac = ((abs & (OneRaw - 1)) * 10000 + (OneRaw >> 1)) >> FractionBits;
        if (frac >= 10000)
        {
            whole++;
            frac -= 10000;
        }
        return (negative ? "-" : "") + whole.ToString(CultureInfo.InvariantCulture) + "." + frac.ToString("D4", CultureInfo.InvariantCulture);
    }
}
