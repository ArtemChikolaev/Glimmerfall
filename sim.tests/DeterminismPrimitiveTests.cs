using System;
using Glimmerfall.Sim.Determinism;
using Xunit;

namespace Glimmerfall.Sim.Tests;

public class Pcg32Tests
{
    /// <summary>Эталон pcg32-demo из pcg-c-basic (seed 42, stream 54), Round 1 — алгоритм совпадает с опубликованным.</summary>
    [Fact]
    public void MatchesReferenceOutput()
    {
        var rng = new Pcg32(42, 54);
        uint[] expected = { 0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e };
        foreach (uint value in expected)
        {
            Assert.Equal(value, rng.NextUInt());
        }
    }

    [Fact]
    public void BoundedIntsStayInRangeAndAreRoughlyUniform()
    {
        var rng = new Pcg32(7, 1);
        var counts = new int[10];
        for (int i = 0; i < 100_000; i++)
        {
            int v = rng.NextInt(10);
            Assert.InRange(v, 0, 9);
            counts[v]++;
        }
        double chiSquare = 0;
        foreach (int c in counts)
        {
            chiSquare += (c - 10_000.0) * (c - 10_000.0) / 10_000.0;
        }
        Assert.True(chiSquare < 27.9, $"chi² = {chiSquare}"); // p ≈ 0.001 при 9 степенях свободы
    }

    [Fact]
    public void ChanceAndFixRangeAreConsistent()
    {
        var rng = new Pcg32(9, 3);
        int hits = 0;
        for (int i = 0; i < 100_000; i++)
        {
            if (rng.Chance(Numerics.Fix.Ratio(1, 4)))
            {
                hits++;
            }
            Assert.InRange(rng.NextFix().Raw, 0, Numerics.Fix.OneRaw - 1);
        }
        Assert.InRange(hits, 24_000, 26_000);
    }

    /// <summary>
    /// Сиды потоков закреплены: смена разведения сидов или порядка потоков сдвигает все забеги — это должно быть
    /// осознанным решением с перегенерацией golden, а не побочным эффектом.
    /// </summary>
    [Fact]
    public void StreamSeedsArePinned()
    {
        var set = new RngSet(SeedHash.FromString("pinned"));
        var first = new uint[RngSet.StreamCount];
        for (int i = 0; i < RngSet.StreamCount; i++)
        {
            first[i] = set[(RngStream)i].NextUInt();
        }
        Assert.Equal(PinnedStreamOutputs, string.Join(",", first));
        Assert.Equal(RngSet.StreamCount, Enum.GetValues<RngStream>().Length);
    }

    /// <summary>Значения сняты при первой реализации (2026-10-26) — это «golden» разведения сидов, а не внешний эталон.</summary>
    private const string PinnedStreamOutputs = "3314132073,2917027656,2838453966,298421707,1163060492,3856491354,3783354834";

    [Fact]
    public void StreamsAreIndependentOfEachOther()
    {
        var a = new RngSet(123);
        var b = new RngSet(123);
        for (int i = 0; i < 1000; i++)
        {
            a[RngStream.Spawn].NextUInt();
        }
        Assert.Equal(b[RngStream.Combat].NextUInt(), a[RngStream.Combat].NextUInt());
    }
}

public class SeedAndHashTests
{
    [Fact]
    public void StringSeedIsStableFnv1a()
    {
        Assert.Equal(14695981039346656037UL, SeedHash.FromString(""));
        Assert.Equal(SeedHash.FromString("daily-2026-10-26"), SeedHash.FromString("daily-2026-10-26"));
        Assert.NotEqual(SeedHash.FromString("daily-2026-10-26"), SeedHash.FromString("daily-2026-10-27"));
    }

    [Fact]
    public void SplitMix64MatchesReference()
    {
        // Первое значение SplitMix64 при состоянии 0 (эталон Vigna, splitmix64.c): 0xE220A8397B1DCDAF.
        Assert.Equal(0xE220A8397B1DCDAFUL, SeedHash.SplitMix64(0));
    }

    [Fact]
    public void HasherIsOrderSensitiveAndPinned()
    {
        var a = new StateHasher();
        a.Add(1);
        a.Add(2);
        var b = new StateHasher();
        b.Add(2);
        b.Add(1);
        Assert.NotEqual(a.Finish(), b.Finish());
        var empty = new StateHasher();
        Assert.Equal(PinnedEmpty, empty.Finish());
        Assert.Equal(PinnedOneTwo, a.Finish());
    }

    /// <summary>Сняты при первой реализации (2026-10-26): закрепляют алгоритм дайджеста.</summary>
    private const ulong PinnedEmpty = 17241709254077376921;
    private const ulong PinnedOneTwo = 14931775778646116012;
}
