using System;
using System.Diagnostics;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Replays;

namespace Glimmerfall.Sim.Cli;

/// <summary>
/// Бенч тиков (G1.3): время шага сима и решения бота отдельно, перцентили, средние числа сущностей и дайджест в конце —
/// перф-оптимизация сравнивает дайджест до и после (скилл sim-determinism).
/// </summary>
public static class BenchCommand
{
    public static int Run(Args args)
    {
        SimConfig config = RunSetup.BuildConfig(args, defaultScenario: "horde", defaultActSeconds: 3600);
        int ticks = args.GetInt("ticks", 3600);
        int warmup = args.GetInt("warmup", 600);
        string seedText = args.Get("seed", "bench");
        IBotPolicy policy = RunSetup.BuildPolicy(args.Get("policy", "kiter"));
        args.EnsureAllUsed();

        var world = new World(config, Determinism.SeedHash.FromString(seedText));
        for (int i = 0; i < warmup && world.Outcome == RunOutcome.InProgress; i++)
        {
            world.Step(policy.Decide(world));
        }

        var stepUs = new double[ticks];
        var botUs = new double[ticks];
        long enemies = 0;
        long projectiles = 0;
        long shots = 0;
        int measured = 0;
        int gcBefore = GC.CollectionCount(0);
        var sw = new Stopwatch();
        while (measured < ticks && world.Outcome == RunOutcome.InProgress)
        {
            sw.Restart();
            InputFrame input = policy.Decide(world);
            double bot = sw.Elapsed.TotalMilliseconds * 1000;
            sw.Restart();
            world.Step(input);
            double step = sw.Elapsed.TotalMilliseconds * 1000;
            if (world.Modal != ModalKind.None)
            {
                continue;
            }
            stepUs[measured] = step;
            botUs[measured] = bot;
            enemies += world.Enemies.Count;
            projectiles += world.Projectiles.Count;
            shots += world.Shots.Count;
            measured++;
        }
        int gc = GC.CollectionCount(0) - gcBefore;
        if (measured == 0)
        {
            Console.WriteLine("nothing measured: the run ended during warmup");
            return 1;
        }
        Array.Resize(ref stepUs, measured);
        Array.Resize(ref botUs, measured);
        Console.WriteLine($"glimmerfall-sim bench · balance {SimVersion.Balance} · {RunSetup.Describe(config)} · seed {seedText}");
        Console.WriteLine($"runtime {Environment.Version} · {System.Runtime.InteropServices.RuntimeInformation.OSDescription} · {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture} · cpus {Environment.ProcessorCount}");
        Console.WriteLine($"measured {measured} ticks after {warmup} warmup · avg enemies {enemies / measured} · projectiles {projectiles / measured} · enemy shots {shots / measured} · gen0 GCs {gc}");
        Console.WriteLine($"sim step ms: {Stats.Describe(stepUs, 1000)}");
        Console.WriteLine($"bot decide ms: {Stats.Describe(botUs, 1000)}");
        Console.WriteLine($"final tick {world.Tick} · digest {Replay.Hex(world.ComputeDigest())}");
        return 0;
    }
}
