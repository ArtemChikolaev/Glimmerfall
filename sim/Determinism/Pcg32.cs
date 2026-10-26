using System;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Determinism;

/// <summary>
/// PCG32 (XSH-RR, O'Neill 2014): 64 бита состояния, 32 бита выхода, 2^63 независимых потоков по инкременту.
/// Алгоритм зафиксирован тестом на эталонные значения pcg32-demo (seed 42, stream 54) — в отличие от
/// <c>System.Random</c> и RNG движка, чей алгоритм — деталь реализации.
/// </summary>
public struct Pcg32
{
    private const ulong Multiplier = 6364136223846793005UL;

    private ulong _state;
    private ulong _increment;

    public Pcg32(ulong seed, ulong stream)
    {
        _state = 0;
        _increment = (stream << 1) | 1UL;
        NextUInt();
        _state += seed;
        NextUInt();
    }

    public readonly ulong State => _state;
    public readonly ulong Increment => _increment;

    public uint NextUInt()
    {
        ulong old = _state;
        _state = old * Multiplier + _increment;
        uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
        int rotation = (int)(old >> 59);
        return (xorShifted >> rotation) | (xorShifted << (-rotation & 31));
    }

    /// <summary>Равномерное целое в [0, bound) без смещения (отбраковка, как pcg32_boundedrand_r).</summary>
    public int NextInt(int bound)
    {
        if (bound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be positive");
        }
        uint b = (uint)bound;
        uint threshold = (0u - b) % b;
        while (true)
        {
            uint r = NextUInt();
            if (r >= threshold)
            {
                return (int)(r % b);
            }
        }
    }

    /// <summary>Равномерное целое в [min, maxExclusive).</summary>
    public int Range(int min, int maxExclusive) => min + NextInt(maxExclusive - min);

    /// <summary>Равномерное значение в [0, 1) с шагом 1/65536.</summary>
    public Fix NextFix() => Fix.FromRaw(NextUInt() >> 16);

    /// <summary>Равномерное значение в [min, max).</summary>
    public Fix Range(Fix min, Fix max) => min + (max - min) * NextFix();

    /// <summary>Событие с вероятностью probability (0..1 в Fix). Всегда тратит ровно одно число.</summary>
    public bool Chance(Fix probability) => (long)(NextUInt() >> 16) < probability.Raw;

    /// <summary>Угол в [0, FullTurn).</summary>
    public int NextAngle() => (int)(NextUInt() >> 16);

    public readonly void AddTo(ref StateHasher hasher)
    {
        hasher.Add(_state);
        hasher.Add(_increment);
    }
}
