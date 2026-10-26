using System;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Replays;

namespace Glimmerfall.Sim.Bots;

/// <summary>Бот завис: тик не растёт N шагов подряд (обычно — модалка, которую политика не разрешает).</summary>
public sealed class BotStallException : Exception
{
    public BotStallException(string message) : base(message)
    {
    }
}

/// <summary>Итог одного прогона для харнесса.</summary>
public readonly struct RunResult
{
    public readonly RunOutcome Outcome;
    public readonly bool TimedOut;
    public readonly int Ticks;
    public readonly int Steps;
    public readonly int ActsReached;
    public readonly int ActsCleared;
    public readonly int Level;
    public readonly int Kills;
    public readonly int KillerKind;
    public readonly ulong Digest;

    public RunResult(World world)
    {
        Outcome = world.Outcome;
        TimedOut = world.Outcome == RunOutcome.InProgress;
        Ticks = world.Tick;
        Steps = world.StepCount;
        int reached = 0;
        int cleared = 0;
        foreach (var act in world.Stats.Acts)
        {
            if (act.Reached)
            {
                reached++;
            }
            if (act.Cleared)
            {
                cleared++;
            }
        }
        ActsReached = reached;
        ActsCleared = cleared;
        Level = world.Level;
        Kills = world.Stats.TotalKills;
        KillerKind = world.Stats.KillerKind;
        Digest = world.ComputeDigest();
    }
}

/// <summary>
/// Цикл прогона бота со сторожами (скилл sim-invariants, правило 1): потолок тиков, потолок шагов и «тик стоит N шагов» —
/// ошибка политики, а не медленный забег. Записывает реплей, если передан рекордер.
/// </summary>
public static class RunDriver
{
    public const int DefaultStallSteps = 600;

    public static RunResult Run(World world, IBotPolicy policy, int maxTicks, ReplayRecorder? recorder = null,
        int stallSteps = DefaultStallSteps)
    {
        long maxSteps = (long)maxTicks * 3 + 1000;
        int lastTick = world.Tick;
        int stalled = 0;
        while (world.Outcome == RunOutcome.InProgress && world.Tick < maxTicks)
        {
            InputFrame input = policy.Decide(world);
            recorder?.Record(input);
            world.Step(input);
            recorder?.AfterStep();
            if (world.Tick != lastTick)
            {
                lastTick = world.Tick;
                stalled = 0;
            }
            else if (++stalled >= stallSteps)
            {
                throw new BotStallException(
                    $"bot '{policy.Name}' stalled: seed={world.Seed} tick={world.Tick} step={world.StepCount} modal={world.Modal} — {stallSteps} steps without a tick");
            }
            if (world.StepCount >= maxSteps)
            {
                throw new BotStallException($"bot '{policy.Name}' exceeded {maxSteps} steps: seed={world.Seed} tick={world.Tick}");
            }
        }
        return new RunResult(world);
    }
}
