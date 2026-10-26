using System;
using System.IO;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Replays;

namespace Glimmerfall.Sim.Cli;

public static class ReplayCommands
{
    /// <summary>Записывает реплей бот-прогона (golden-реплеи sim.tests/golden пишутся этой командой).</summary>
    public static int Record(Args args)
    {
        SimConfig config = RunSetup.BuildConfig(args, defaultActs: 1, defaultActSeconds: 120);
        string policyName = args.Get("policy", "kiter");
        string seed = args.Get("seed", "record");
        int every = args.GetInt("every", 600);
        string output = args.Get("out", "");
        int defaultMaxTicks = config.Scenario == Scenario.HordeBench ? 600 : config.Acts * (config.ActTicks + 120 * SimConstants.TicksPerSecond);
        int maxTicks = args.GetInt("max-ticks", defaultMaxTicks);
        args.EnsureAllUsed();
        if (output.Length == 0)
        {
            throw new ArgumentException("--out FILE is required");
        }
        var world = new World(config, SeedHash.FromString(seed));
        var recorder = new ReplayRecorder(world, policyName, every);
        RunDriver.Run(world, RunSetup.BuildPolicy(policyName), maxTicks, recorder);
        Replay replay = recorder.Finish();
        string text = "# seed text: " + seed + "\n" + replay.ToText();
        File.WriteAllText(output, text);
        Console.WriteLine($"recorded {output}: {replay.FinalSteps} steps, {replay.FinalTicks} ticks, {replay.Outcome}, {replay.Inputs.Count} input changes, digest {Replay.Hex(replay.FinalDigest)}");
        return 0;
    }

    /// <summary>Ре-симулирует реплеи и сверяет дайджесты; код выхода 1 при любом расхождении или отказе.</summary>
    public static int Verify(Args args)
    {
        args.EnsureAllUsed();
        if (args.Positional.Count == 0)
        {
            throw new ArgumentException("verify needs replay files");
        }
        int failures = 0;
        foreach (string path in args.Positional)
        {
            try
            {
                VerifyResult result = ReplayPlayer.Verify(Replay.Parse(File.ReadAllText(path)));
                Console.WriteLine($"{(result.Ok ? "OK  " : "FAIL")} {path}: {result.Message}");
                if (!result.Ok)
                {
                    failures++;
                }
            }
            catch (Exception e) when (e is ReplayIncompatibleException or FormatException)
            {
                Console.WriteLine($"FAIL {path}: {e.Message}");
                failures++;
            }
        }
        return failures == 0 ? 0 : 1;
    }
}
