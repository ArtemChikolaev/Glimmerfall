namespace Glimmerfall.Sim.Determinism;

/// <summary>Стабильные хеши сидов. Не <c>string.GetHashCode</c>: в .NET он рандомизирован на процесс.</summary>
public static class SeedHash
{
    private const ulong FnvOffset = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

    /// <summary>FNV-1a 64 по UTF-16 единицам строки (младший байт, затем старший).</summary>
    public static ulong FromString(string text)
    {
        ulong hash = FnvOffset;
        foreach (char c in text)
        {
            hash ^= (byte)c;
            hash *= FnvPrime;
            hash ^= (byte)(c >> 8);
            hash *= FnvPrime;
        }
        return hash;
    }

    /// <summary>SplitMix64 (Steele, Lea, Flood 2014) — разведение одного сида в независимые.</summary>
    public static ulong SplitMix64(ulong value)
    {
        ulong z = value + 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    /// <summary>Сид подпотока: зависит только от базового сида, номера потока и индекса (места, акта и т. п.).</summary>
    public static ulong Derive(ulong runSeed, ulong stream, ulong index) =>
        SplitMix64(SplitMix64(runSeed ^ SplitMix64(stream)) ^ SplitMix64(index + 0x632BE59BD9B4E019UL));
}
