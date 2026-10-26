using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Glimmerfall.Sim;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Замер кадра для Steam Deck и телефонов (G1.3/G1.4): время кадра (дельта между кадрами при выключенном vsync),
/// своя работа (сим + бот + заливка буферов), шаг сима, CPU/GPU-время рендера вьюпорта. Отчёт — в stdout и user://.
/// </summary>
public sealed class FrameStats
{
    private readonly List<double> _frameMs = new();
    private readonly List<double> _scriptMs = new();
    private readonly List<double> _simMs = new();
    private readonly List<double> _renderCpuMs = new();
    private readonly List<double> _renderGpuMs = new();
    private long _enemies;
    private long _projectiles;
    private long _instances;
    private long _drawCalls;

    public int Frames => _frameMs.Count;

    public void Record(double frameMs, double scriptMs, double simMs, double renderCpuMs, double renderGpuMs, World world, int instances)
    {
        _frameMs.Add(frameMs);
        _scriptMs.Add(scriptMs);
        _simMs.Add(simMs);
        _renderCpuMs.Add(renderCpuMs);
        _renderGpuMs.Add(renderGpuMs);
        _enemies += world.Enemies.Count;
        _projectiles += world.Projectiles.Count;
        _instances += instances;
        _drawCalls += (long)RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalDrawCallsInFrame);
    }

    public string Report(SpikeOptions options)
    {
        var sb = new StringBuilder();
        int n = Math.Max(1, Frames);
        sb.AppendLine("glimmerfall horde spike bench");
        sb.AppendLine($"godot {Engine.GetVersionInfo()["string"]} · renderer {RenderingServer.GetCurrentRenderingMethod()} / {RenderingServer.GetCurrentRenderingDriverName()} · .NET {System.Environment.Version}");
        sb.AppendLine($"device {OS.GetName()} {OS.GetModelName()} · cpu {OS.GetProcessorName()} × {OS.GetProcessorCount()} · gpu {RenderingServer.GetVideoAdapterVendor()} {RenderingServer.GetVideoAdapterName()}");
        sb.AppendLine($"window {DisplayServer.WindowGetSize()} · vsync {DisplayServer.WindowGetVsyncMode()} · scenario {options.Scenario} · seed {options.Seed} · balance {SimVersion.Balance}");
        sb.AppendLine($"frames {Frames} · avg enemies {_enemies / n} · projectiles {_projectiles / n} · instances {_instances / n} · draw calls {_drawCalls / n}");
        sb.AppendLine($"frame ms        {Describe(_frameMs)}");
        sb.AppendLine($"script ms       {Describe(_scriptMs)}");
        sb.AppendLine($"sim ms          {Describe(_simMs)}");
        sb.AppendLine($"render cpu ms   {Describe(_renderCpuMs)}");
        sb.AppendLine($"render gpu ms   {Describe(_renderGpuMs)}");
        sb.AppendLine($"frames over 8.0 ms: {Share(_frameMs, 8.0)} · over 16.7 ms: {Share(_frameMs, 16.7)}");
        return sb.ToString();
    }

    /// <summary>Строка для HUD: среднее и p99 кадра, среднее сима и GPU.</summary>
    public string Compact()
    {
        if (Frames == 0)
        {
            return "—";
        }
        double p99 = _frameMs.OrderBy(v => v).ElementAt(Math.Min(Frames - 1, (int)(0.99 * Frames)));
        return string.Format(CultureInfo.InvariantCulture, "frame {0:0.00} ms (p99 {1:0.00}) · sim {2:0.00} ms · gpu {3:0.00} ms",
            _frameMs.Average(), p99, _simMs.Average(), _renderGpuMs.Average());
    }

    private static string Describe(List<double> values)
    {
        if (values.Count == 0)
        {
            return "—";
        }
        double[] sorted = values.OrderBy(v => v).ToArray();
        double Q(double q) => sorted[Math.Min(sorted.Length - 1, (int)(q * sorted.Length))];
        return string.Format(CultureInfo.InvariantCulture, "mean {0,7:0.000} · p50 {1,7:0.000} · p95 {2,7:0.000} · p99 {3,7:0.000} · max {4,7:0.000}",
            values.Average(), Q(0.5), Q(0.95), Q(0.99), sorted[^1]);
    }

    private static string Share(List<double> values, double threshold) =>
        values.Count == 0 ? "—" : string.Format(CultureInfo.InvariantCulture, "{0:0.00}%", 100.0 * values.Count(v => v > threshold) / values.Count);
}
