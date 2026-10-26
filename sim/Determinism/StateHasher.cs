namespace Glimmerfall.Sim.Determinism;

/// <summary>
/// Потоковый 64-битный дайджест состояния сима: раунд и лавина xxHash64 по 64-битным словам.
/// Не криптография — отпечаток для сравнения прогонов на разных ОС, сервере и в реплеях.
/// Порядок добавления полей — часть формата: меняешь — перегенерируй golden осознанно.
/// </summary>
public struct StateHasher
{
    private const ulong Prime1 = 11400714785074694791UL;
    private const ulong Prime2 = 14029467366897019727UL;
    private const ulong Prime3 = 1609587929392839161UL;
    private const ulong Prime4 = 9650029242287828579UL;
    private const ulong Prime5 = 2870177450012600261UL;

    private ulong _accumulator;
    private ulong _count;
    private bool _started;

    public void Add(ulong value)
    {
        if (!_started)
        {
            _accumulator = Prime5;
            _started = true;
        }
        ulong k = value * Prime2;
        k = (k << 31) | (k >> 33);
        k *= Prime1;
        _accumulator ^= k;
        _accumulator = ((_accumulator << 27) | (_accumulator >> 37)) * Prime1 + Prime4;
        _count++;
    }

    public void Add(long value) => Add((ulong)value);
    public void Add(int value) => Add((ulong)(long)value);
    public void Add(uint value) => Add((ulong)value);
    public void Add(bool value) => Add(value ? 1UL : 0UL);

    public readonly ulong Finish()
    {
        ulong h = (_started ? _accumulator : Prime5) + _count * 8;
        h ^= h >> 33;
        h *= Prime2;
        h ^= h >> 29;
        h *= Prime3;
        h ^= h >> 32;
        return h;
    }
}
