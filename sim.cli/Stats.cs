using System;
using System.Collections.Generic;
using System.Linq;

namespace Glimmerfall.Sim.Cli;

/// <summary>Перцентили и доли для отчётов харнесса. Здесь float можно: это отчёт, а не сим.</summary>
public static class Stats
{
    public static double Quantile(IReadOnlyList<double> values, double q)
    {
        if (values.Count == 0)
        {
            return double.NaN;
        }
        double[] sorted = values.OrderBy(v => v).ToArray();
        int index = (int)Math.Min(sorted.Length - 1, Math.Floor(q * sorted.Length));
        return sorted[index];
    }

    public static string Describe(double[] values, double divisor)
    {
        double mean = values.Average() / divisor;
        return $"mean {mean:0.000} · p50 {Quantile(values, 0.5) / divisor:0.000} · p95 {Quantile(values, 0.95) / divisor:0.000} · p99 {Quantile(values, 0.99) / divisor:0.000} · max {values.Max() / divisor:0.000}";
    }

    /// <summary>Доля с доверительной полосой ±1 стандартная ошибка: «41.7% ±8.2 (15/36)».</summary>
    public static string Rate(int hits, int total)
    {
        if (total == 0)
        {
            return "—";
        }
        double p = (double)hits / total;
        double se = Math.Sqrt(p * (1 - p) / total);
        return $"{p * 100:0.0}% ±{se * 100:0.0} ({hits}/{total})";
    }
}
