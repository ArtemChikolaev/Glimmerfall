namespace Glimmerfall.Sim.Numerics;

/// <summary>
/// Тригонометрия и корни без libm: таблицы <see cref="TrigTables"/> с линейной интерполяцией и целочисленный корень.
/// Угол — целое в «двоичных градусах»: <see cref="FullTurn"/> = 65536 единиц на оборот, 0 — вдоль +X, рост к +Y.
/// </summary>
public static class FixMath
{
    public const int FullTurn = 1 << 16;
    public const int HalfTurn = FullTurn / 2;
    public const int QuarterTurn = FullTurn / 4;
    private const int AngleMask = FullTurn - 1;
    private const int QuarterShift = 14; // QuarterTurn = 2^14
    private const int SinStepShift = 4; // 16384 единиц четверти / 1024 шага = 16 = 2^4
    private const int AtanStepShift = 6; // 65536 (t = 1 в Q16) / 1024 шага = 64 = 2^6

    /// <summary>Градусы → единицы угла (целые градусы, для констант контента).</summary>
    public static int Degrees(int degrees) => (int)((long)degrees * FullTurn / 360);

    public static Fix Sin(int angle)
    {
        int a = angle & AngleMask;
        int quadrant = a >> QuarterShift;
        int within = a & (QuarterTurn - 1);
        if ((quadrant & 1) != 0)
        {
            within = QuarterTurn - within;
        }
        int index = within >> SinStepShift;
        int frac = within & ((1 << SinStepShift) - 1);
        int s0 = TrigTables.SinQuarter[index];
        int s1 = index < TrigTables.Steps ? TrigTables.SinQuarter[index + 1] : s0;
        long value = s0 + (((long)(s1 - s0) * frac) >> SinStepShift);
        return Fix.FromRaw(quadrant >= 2 ? -value : value);
    }

    public static Fix Cos(int angle) => Sin(angle + QuarterTurn);

    /// <summary>Единичный вектор направления угла.</summary>
    public static FixVec2 Direction(int angle) => new(Cos(angle), Sin(angle));

    /// <summary>Угол вектора (x, y) в [0, FullTurn). Для (0, 0) — 0. Аргументы — сырые значения в одних единицах.</summary>
    public static int Atan2(long y, long x)
    {
        if (x == 0 && y == 0)
        {
            return 0;
        }
        long ax = x < 0 ? -x : x;
        long ay = y < 0 ? -y : y;
        bool steep = ay > ax;
        long num = steep ? ax : ay;
        long den = steep ? ay : ax;
        while (den > (1L << 46))
        {
            num >>= 1;
            den >>= 1;
        }
        int t = (int)((num << Fix.FractionBits) / den); // 0..65536 — тангенс угла октанта
        int index = t >> AtanStepShift;
        int frac = t & ((1 << AtanStepShift) - 1);
        int a0 = TrigTables.AtanOctant[index];
        int a1 = index < TrigTables.Steps ? TrigTables.AtanOctant[index + 1] : a0;
        int angle = a0 + (((a1 - a0) * frac) >> AtanStepShift);
        if (steep)
        {
            angle = QuarterTurn - angle;
        }
        if (x < 0)
        {
            angle = HalfTurn - angle;
        }
        if (y < 0)
        {
            angle = FullTurn - angle;
        }
        return angle & AngleMask;
    }

    /// <summary>Целая часть квадратного корня (метод Ньютона сверху; точный floor).</summary>
    public static ulong ISqrt(ulong n)
    {
        if (n < 2)
        {
            return n;
        }
        int bits = 64 - LeadingZeroCount(n);
        ulong x = 1UL << ((bits + 1) >> 1);
        while (true)
        {
            ulong y = (x + n / x) >> 1;
            if (y >= x)
            {
                return x;
            }
            x = y;
        }
    }

    /// <summary>Число ведущих нулевых бит (в netstandard2.1 нет BitOperations).</summary>
    public static int LeadingZeroCount(ulong value)
    {
        if (value == 0)
        {
            return 64;
        }
        int n = 0;
        if (value <= 0x00000000FFFFFFFFUL) { n += 32; value <<= 32; }
        if (value <= 0x0000FFFFFFFFFFFFUL) { n += 16; value <<= 16; }
        if (value <= 0x00FFFFFFFFFFFFFFUL) { n += 8; value <<= 8; }
        if (value <= 0x0FFFFFFFFFFFFFFFUL) { n += 4; value <<= 4; }
        if (value <= 0x3FFFFFFFFFFFFFFFUL) { n += 2; value <<= 2; }
        if (value <= 0x7FFFFFFFFFFFFFFFUL) { n += 1; }
        return n;
    }
}
