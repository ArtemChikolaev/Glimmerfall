using System;
using System.IO;
using System.Text;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Replays;

namespace Glimmerfall.Sim.Cli;

/// <summary>
/// Набор прогонов для сверки между ОС (матрица CI Windows x64 / Linux x64 / macOS arm64): кайтер и AFK на коротком акте
/// и спайк орды. Каждая ОС печатает один и тот же текст; джоб CI сравнивает файлы побайтно — без доверия к golden.
/// </summary>
public static class DigestCommand
{
    public static int Run(Args args)
    {
        string output = args.Get("out", "");
        args.EnsureAllUsed();
        var sb = new StringBuilder();
        sb.Append("balance ").Append(SimVersion.Balance).Append('\n');
        for (int i = 0; i < 3; i++)
        {
            Append(sb, "kiter-act", RunConfig(), "ci-kiter-" + i, new KiterPolicy(), 180 * SimConstants.TicksPerSecond);
            Append(sb, "afk-act", RunConfig(), "ci-afk-" + i, new AfkPolicy(), 180 * SimConstants.TicksPerSecond);
        }
        Append(sb, "horde", SimConfig.HordeBench(), "ci-horde", new KiterPolicy(), 900);
        string text = sb.ToString();
        Console.Write(text);
        if (output.Length > 0)
        {
            File.WriteAllText(output, text);
        }
        return 0;
    }

    private static SimConfig RunConfig()
    {
        SimConfig config = SimConfig.Run(acts: 1);
        config.ActTicks = 120 * SimConstants.TicksPerSecond;
        config.BossLeadTicks = 30 * SimConstants.TicksPerSecond;
        config.EliteFirstTicks = 40 * SimConstants.TicksPerSecond;
        config.ElitePeriodTicks = 40 * SimConstants.TicksPerSecond;
        return config;
    }

    private static void Append(StringBuilder sb, string name, SimConfig config, string seed, IBotPolicy policy, int maxTicks)
    {
        var world = new World(config, SeedHash.FromString(seed));
        RunResult result = RunDriver.Run(world, policy, maxTicks);
        sb.Append(name).Append(' ').Append(seed)
            .Append(" steps=").Append(result.Steps)
            .Append(" ticks=").Append(result.Ticks)
            .Append(" outcome=").Append(result.Outcome)
            .Append(" kills=").Append(result.Kills)
            .Append(" digest=").Append(Replay.Hex(result.Digest))
            .Append('\n');
    }
}
