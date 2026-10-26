namespace Glimmerfall.Sim.Determinism;

/// <summary>
/// Потоки случайности по подсистемам (скилл sim-determinism, правило 2). Новый потребитель — новый поток
/// в конец перечисления: сид потока зависит только от сида забега и номера, поэтому остальные не сдвигаются.
/// </summary>
public enum RngStream
{
    Spawn = 0,
    Combat = 1,
    Offers = 2,
    Ai = 3,
    Loot = 4,
    Places = 5,
    Draft = 6,
}

/// <summary>Набор генераторов забега, по одному на <see cref="RngStream"/>.</summary>
public sealed class RngSet
{
    public const int StreamCount = 7;

    private readonly Pcg32[] _streams = new Pcg32[StreamCount];

    public RngSet(ulong runSeed)
    {
        for (int i = 0; i < StreamCount; i++)
        {
            _streams[i] = new Pcg32(SeedHash.Derive(runSeed, (ulong)i, 0), (ulong)i + 1);
        }
    }

    public ref Pcg32 this[RngStream stream] => ref _streams[(int)stream];

    public void AddTo(ref StateHasher hasher)
    {
        for (int i = 0; i < StreamCount; i++)
        {
            _streams[i].AddTo(ref hasher);
        }
    }
}
