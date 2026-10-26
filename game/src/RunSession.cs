using System;
using System.Collections.Generic;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;

namespace Glimmerfall.Game;

/// <summary>
/// Один забег: мир сима и его отрисовка. Сим шагает фиксированным тиком 60 Гц из аккумулятора кадра, рендер идёт на
/// частоте дисплея с интерполяцией. Ввод — автопилот-кайтер из Sim.dll или игрок; события тика уходят слушателям
/// (частицы, звук, цифры урона). Пауза — просто не звать <see cref="Advance"/>.
/// </summary>
public sealed class RunSession
{
    public const double TickSeconds = 1.0 / SimConstants.TicksPerSecond;
    private const int MaxStepsPerFrame = 6;

    private double _accumulator;

    public RunSession(SimConfig config, string seed, WorldHost host, int? particleSeed, bool autopilot)
    {
        Seed = seed;
        World = new World(config, SeedHash.FromString(seed)) { CollectEvents = true };
        View = new WorldView();
        host.World.AddChild(View);
        View.Init(World, host, particleSeed);
        Autopilot = autopilot ? new KiterPolicy() : null;
    }

    public World World { get; }
    public WorldView View { get; }
    public string Seed { get; }
    public IBotPolicy? Autopilot { get; set; }

    /// <summary>Доля тика для интерполяции; на модалке мир стоит — без интерполяции.</summary>
    public float Alpha => World.Modal == ModalKind.None ? (float)(_accumulator / TickSeconds) : 1f;

    /// <summary>Шагает сим за прошедшее время; возвращает число шагов и чистое время сима.</summary>
    public int Advance(double delta, Func<World, InputFrame> player, Action<List<SimEvent>> onEvents, out double simMs)
    {
        _accumulator += Math.Min(delta, 0.25);
        int steps = 0;
        long simTicks = 0;
        while (_accumulator >= TickSeconds && steps < MaxStepsPerFrame)
        {
            InputFrame input = Autopilot?.Decide(World) ?? player(World);
            long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
            World.Step(input);
            simTicks += System.Diagnostics.Stopwatch.GetTimestamp() - t0;
            onEvents(World.Events);
            _accumulator -= TickSeconds;
            steps++;
        }
        if (steps == MaxStepsPerFrame)
        {
            _accumulator = 0;
        }
        simMs = simTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
        return steps;
    }

    public void Dispose() => View.QueueFree();
}
