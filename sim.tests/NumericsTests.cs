using System;
using System.Numerics;
using Glimmerfall.Sim.Numerics;
using Xunit;

namespace Glimmerfall.Sim.Tests;

public class FixTests
{
    [Fact]
    public void ArithmeticMatchesQ16Semantics()
    {
        Assert.Equal(Fix.FromInt(3), Fix.Ratio(3, 2) * Fix.FromInt(2));
        Assert.Equal(Fix.Ratio(7, 2), Fix.FromInt(7) / Fix.FromInt(2));
        Assert.Equal(Fix.Ratio(-3, 2), Fix.Ratio(-3, 2) * Fix.One);
        Assert.Equal(Fix.One, Fix.PerSecond(SimConstants.TicksPerSecond));
        Assert.Equal(Fix.Ratio(3, 2), Fix.Milli(1500));
        Assert.Equal(98304, Fix.Milli(1500).Raw);
    }

    [Theory]
    [InlineData(98304, 1, 2, 2)] // 1.5
    [InlineData(-98304, -2, -1, -1)] // -1.5: floor −2, ceil −1, round half up −1
    [InlineData(65535, 0, 1, 1)]
    public void Rounding(long raw, int floor, int ceil, int round)
    {
        Fix value = Fix.FromRaw(raw);
        Assert.Equal(floor, value.FloorToInt());
        Assert.Equal(ceil, value.CeilToInt());
        Assert.Equal(round, value.RoundToInt());
    }

    [Fact]
    public void SqrtIsFloorOfExactRoot()
    {
        var random = new Random(1);
        for (int i = 0; i < 20000; i++)
        {
            long raw = random.NextInt64(0, 1L << 46);
            long root = Fix.Sqrt(Fix.FromRaw(raw)).Raw;
            // root = floor(sqrt(raw · 2^16)) — проверяем точно, в BigInteger.
            BigInteger n = new BigInteger(raw) << 16;
            Assert.True(new BigInteger(root) * root <= n, $"raw {raw}");
            Assert.True(new BigInteger(root + 1) * (root + 1) > n, $"raw {raw}");
        }
    }

    [Fact]
    public void ISqrtIsExactOverWholeRange()
    {
        var random = new Random(2);
        ulong[] edges = { 0, 1, 2, 3, 4, 15, 16, 17, ulong.MaxValue, ulong.MaxValue - 1, 1UL << 62, (1UL << 63) + 12345, 0xFFFFFFFE00000001UL };
        foreach (ulong n in edges)
        {
            AssertFloorSqrt(n);
        }
        for (int i = 0; i < 20000; i++)
        {
            AssertFloorSqrt((ulong)random.NextInt64() << random.Next(0, 2));
        }
    }

    private static void AssertFloorSqrt(ulong n)
    {
        ulong x = FixMath.ISqrt(n);
        Assert.True((BigInteger)x * x <= n, $"n {n}");
        Assert.True((BigInteger)(x + 1) * (x + 1) > n, $"n {n}");
    }

    [Fact]
    public void LeadingZeroCountMatchesRuntime()
    {
        var random = new Random(3);
        Assert.Equal(64, FixMath.LeadingZeroCount(0));
        for (int i = 0; i < 10000; i++)
        {
            ulong v = (ulong)random.NextInt64() >> random.Next(0, 63);
            Assert.Equal(BitOperations.LeadingZeroCount(v), FixMath.LeadingZeroCount(v));
        }
    }

    [Theory]
    [InlineData(98304, "1.5000")]
    [InlineData(-98304, "-1.5000")]
    [InlineData(1, "0.0000")]
    [InlineData(65536 * 3 + 6554, "3.1000")]
    [InlineData(long.MinValue, "-140737488355328.0000")]
    public void ToStringUsesIntegerArithmetic(long raw, string expected)
    {
        Assert.Equal(expected, Fix.FromRaw(raw).ToString());
    }
}

public class TrigTests
{
    [Fact]
    public void SinTableMatchesFormula()
    {
        for (int i = 0; i <= TrigTables.Steps; i++)
        {
            double exact = Math.Sin(i * (Math.PI / 2) / TrigTables.Steps) * 65536;
            Assert.True(Math.Abs(TrigTables.SinQuarter[i] - exact) <= 0.5 + 1e-6, $"sin entry {i}: {TrigTables.SinQuarter[i]} vs {exact}");
        }
    }

    [Fact]
    public void AtanTableMatchesFormula()
    {
        for (int i = 0; i <= TrigTables.Steps; i++)
        {
            double exact = Math.Atan((double)i / TrigTables.Steps) * 65536 / (2 * Math.PI);
            Assert.True(Math.Abs(TrigTables.AtanOctant[i] - exact) <= 0.5 + 1e-6, $"atan entry {i}: {TrigTables.AtanOctant[i]} vs {exact}");
        }
    }

    [Fact]
    public void SinAndCosAreAccurateForEveryAngle()
    {
        for (int angle = 0; angle < FixMath.FullTurn; angle++)
        {
            double radians = angle * 2 * Math.PI / FixMath.FullTurn;
            Assert.True(Math.Abs(FixMath.Sin(angle).Raw - Math.Sin(radians) * 65536) <= 2, $"sin {angle}");
            Assert.True(Math.Abs(FixMath.Cos(angle).Raw - Math.Cos(radians) * 65536) <= 2, $"cos {angle}");
        }
    }

    [Fact]
    public void SinWrapsAndIsOdd()
    {
        Assert.Equal(Fix.Zero, FixMath.Sin(0));
        Assert.Equal(Fix.One, FixMath.Sin(FixMath.QuarterTurn));
        Assert.Equal(-Fix.One, FixMath.Sin(3 * FixMath.QuarterTurn));
        Assert.Equal(FixMath.Sin(123), FixMath.Sin(123 + FixMath.FullTurn));
        Assert.Equal(-FixMath.Sin(123), FixMath.Sin(-123));
    }

    [Fact]
    public void Atan2IsAccurateAroundTheCircle()
    {
        var random = new Random(4);
        for (int i = 0; i < 50000; i++)
        {
            long x = random.NextInt64(-(1L << 32), 1L << 32);
            long y = random.NextInt64(-(1L << 32), 1L << 32);
            if (x == 0 && y == 0)
            {
                continue;
            }
            double exact = Math.Atan2(y, x) * FixMath.FullTurn / (2 * Math.PI);
            if (exact < 0)
            {
                exact += FixMath.FullTurn;
            }
            double error = Math.Abs(FixMath.Atan2(y, x) - exact);
            error = Math.Min(error, FixMath.FullTurn - error);
            Assert.True(error <= 3, $"atan2({y}, {x}) = {FixMath.Atan2(y, x)} vs {exact}");
        }
    }

    [Fact]
    public void DirectionIsUnitLength()
    {
        for (int angle = 0; angle < FixMath.FullTurn; angle += 97)
        {
            long length = FixMath.Direction(angle).Length.Raw;
            Assert.InRange(length, Fix.OneRaw - 3, Fix.OneRaw + 3);
        }
    }
}
