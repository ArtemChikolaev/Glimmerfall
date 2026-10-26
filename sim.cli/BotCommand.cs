using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Core;

namespace Glimmerfall.Sim.Cli;

/// <summary>Снимок одного прогона для отчёта и JSON-дампа (офлайн-свипы — Python, скилл balance-harness).</summary>
public sealed class RunRecord
{
    public int Index { get; set; }
    public string Seed { get; set; } = "";
    public string Outcome { get; set; } = "";
    public int Ticks { get; set; }
    public int Level { get; set; }
    public int Kills { get; set; }
    public string Killer { get; set; } = "";
    public string Digest { get; set; } = "";
    public List<ActRecord> Acts { get; set; } = new();
}

public sealed class ActRecord
{
    public bool Reached { get; set; }
    public bool Cleared { get; set; }
    public int Seconds { get; set; }
    public int WipeSecond { get; set; }
    public int Kills { get; set; }
    public int EliteKills { get; set; }
    public int DamageTaken { get; set; }
    public long DamageDealt { get; set; }
    public int HeroDeaths { get; set; }
    public int Swaps { get; set; }
    public int Dashes { get; set; }
    public int Assists { get; set; }
    public int Duets { get; set; }
    public int Shrines { get; set; }
    public int Altars { get; set; }
    public int LevelAtEnd { get; set; }
    public int PeakEnemies { get; set; }
}

public static class BotCommand
{
    public static int Run(Args args)
    {
        SimConfig template = RunSetup.BuildConfig(args);
        string policyName = args.Get("policy", "kiter");
        int runs = args.GetInt("runs", 36);
        string baseSeed = args.Get("seed", "bot");
        int jobs = args.GetInt("jobs", Environment.ProcessorCount);
        int maxTicks = args.GetInt("max-minutes", template.Acts * 12) * 60 * SimConstants.TicksPerSecond;
        string dump = args.Get("dump", "");
        bool verbose = args.Has("verbose");
        int trace = args.GetInt("trace", -1);
        int gateWipe = args.GetInt("gate-wipe-p50", -1);
        args.EnsureAllUsed();
        RunSetup.BuildPolicy(policyName);
        if (trace >= 0)
        {
            return Trace(template, policyName, baseSeed, trace, maxTicks);
        }

        var watch = Stopwatch.StartNew();
        var records = new RunRecord[runs];
        Parallel.For(0, runs, new ParallelOptions { MaxDegreeOfParallelism = jobs }, i =>
        {
            SimConfig config = Clone(template);
            var world = new World(config, RunSetup.RunSeed(baseSeed, i));
            RunResult result = RunDriver.Run(world, RunSetup.BuildPolicy(policyName), maxTicks);
            records[i] = ToRecord(i, baseSeed, world, result);
        });
        watch.Stop();

        Console.Write(Report(template, policyName, baseSeed, records, watch.Elapsed.TotalSeconds));
        if (verbose)
        {
            foreach (RunRecord r in records)
            {
                Console.WriteLine($"#{r.Index} {r.Outcome} {RunSetup.Clock(r.Ticks)} lvl {r.Level} kills {r.Kills} killer {r.Killer} digest {r.Digest}");
            }
        }
        if (dump.Length > 0)
        {
            File.WriteAllText(dump, JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"json → {dump}");
        }
        return gateWipe >= 0 ? WipeGate(records, gateWipe) : 0;
    }

    /// <summary>
    /// Гейт угрозы (столп 2, скилл balance-harness): каждый прогон гибнет в первом акте, медиана гибели — не позже порога.
    /// Для AFK-политик: стоять нельзя уже на первой минуте. Медиана — та же, что в отчёте.
    /// </summary>
    private static int WipeGate(RunRecord[] records, int seconds)
    {
        var wipes = records.Where(r => r.Acts.Count > 0 && r.Acts[0].WipeSecond >= 0).Select(r => (double)r.Acts[0].WipeSecond).ToList();
        int survivors = records.Length - wipes.Count;
        double median = wipes.Count > 0 ? Stats.Quantile(wipes, 0.5) : -1;
        bool ok = records.Length > 0 && survivors == 0 && median <= seconds;
        Console.WriteLine(ok
            ? $"gate ok: all {records.Length} runs wiped in act 1, wipe p50 {Sec(median)} ≤ {seconds} s"
            : $"GATE FAILED: act 1 survivors {survivors}/{records.Length}, wipe p50 {(median < 0 ? "—" : Sec(median))}, limit {seconds} s");
        return ok ? 0 : 1;
    }

    /// <summary>Посекундная трасса одного прогона: здоровье, враги рядом, босс, модалки — для отладки политики бота.</summary>
    private static int Trace(SimConfig template, string policyName, string baseSeed, int index, int maxTicks)
    {
        var world = new World(Clone(template), RunSetup.RunSeed(baseSeed, index));
        IBotPolicy policy = RunSetup.BuildPolicy(policyName);
        Console.WriteLine($"trace {baseSeed}-{index} · {RunSetup.Describe(template)}");
        while (world.Outcome == RunOutcome.InProgress && world.Tick < maxTicks)
        {
            int tick = world.Tick;
            world.Step(policy.Decide(world));
            if (world.Tick == tick || world.Tick % SimConstants.TicksPerSecond != 0)
            {
                continue;
            }
            Squad s = world.Squad;
            int near = 0;
            for (int i = 0; i < world.Enemies.HighWater; i++)
            {
                if (world.Enemies.Alive[i])
                {
                    long dx = (world.Enemies.X[i] - s.X).Raw >> Numerics.Fix.FractionBits;
                    long dy = (world.Enemies.Y[i] - s.Y).Raw >> Numerics.Fix.FractionBits;
                    if (dx * dx + dy * dy < 100 * 100)
                    {
                        near++;
                    }
                }
            }
            string boss = "-";
            if (world.IsBossAlive)
            {
                int b = world.BossHandle & 0xFFFF;
                long dx = (world.Enemies.X[b] - s.X).Raw >> Numerics.Fix.FractionBits;
                long dy = (world.Enemies.Y[b] - s.Y).Raw >> Numerics.Fix.FractionBits;
                boss = $"hp {world.Enemies.Hp[b]} d {(int)Math.Sqrt(dx * dx + dy * dy)} {world.Enemies.State[b]}";
            }
            Console.WriteLine($"{RunSetup.Clock(world.Tick)} act {world.ActIndex + 1} {RunSetup.Clock(world.ActTick)} · hero {s.Active} hp {s.Hp[s.Active]}/{s.MaxHp[s.Active]} alive {s.AliveCount} · lvl {world.Level} · enemies {world.Enemies.Count} near100 {near} · pos {s.X.FloorToInt()},{s.Y.FloorToInt()} · boss {boss}");
        }
        Console.WriteLine($"end: {world.Outcome} at {RunSetup.Clock(world.Tick)} · killer {RunSetup.EnemyName(world.Config, world.Stats.KillerKind)}");
        return 0;
    }

    /// <summary>Свежий конфиг с теми же полями и оверрайдами: прогоны не делят изменяемые данные.</summary>
    public static SimConfig Clone(SimConfig template) => SimConfig.FromHeader(template.ToHeader(), template.Overrides);

    public static RunRecord ToRecord(int index, string baseSeed, World world, RunResult result)
    {
        var record = new RunRecord
        {
            Index = index,
            Seed = baseSeed + "-" + index,
            Outcome = result.TimedOut ? "timeout" : result.Outcome == RunOutcome.Victory ? "victory" : "defeat",
            Ticks = result.Ticks,
            Level = result.Level,
            Kills = result.Kills,
            Killer = RunSetup.EnemyName(world.Config, result.KillerKind),
            Digest = Replays.Replay.Hex(result.Digest),
        };
        foreach (ActStats act in world.Stats.Acts)
        {
            int end = act.EndTick >= 0 ? act.EndTick : world.Tick;
            record.Acts.Add(new ActRecord
            {
                Reached = act.Reached,
                Cleared = act.Cleared,
                Seconds = act.Reached ? (end - act.StartTick) / SimConstants.TicksPerSecond : 0,
                WipeSecond = act.WipeActTick >= 0 ? act.WipeActTick / SimConstants.TicksPerSecond : -1,
                Kills = act.Kills,
                EliteKills = act.EliteKills,
                DamageTaken = act.DamageTaken,
                DamageDealt = act.DamageDealt,
                HeroDeaths = act.HeroDeaths,
                Swaps = act.Swaps,
                Dashes = act.Dashes,
                Assists = act.Assists,
                Duets = act.Duets,
                Shrines = act.Shrines,
                Altars = act.Altars,
                LevelAtEnd = act.Cleared || act.WipeActTick >= 0 ? act.LevelAtEnd : world.Level,
                PeakEnemies = act.PeakEnemies,
            });
        }
        return record;
    }

    public static string Report(SimConfig config, string policy, string baseSeed, RunRecord[] records, double seconds)
    {
        var sb = new StringBuilder();
        int n = records.Length;
        sb.AppendLine($"glimmerfall-sim bot · balance {SimVersion.Balance} · policy {policy} · runs {n} · seed {baseSeed}-* · {RunSetup.Describe(config)} · {seconds:0.0} s");
        int wins = records.Count(r => r.Outcome == "victory");
        int defeats = records.Count(r => r.Outcome == "defeat");
        int timeouts = records.Count(r => r.Outcome == "timeout");
        sb.AppendLine($"outcome: victory {Stats.Rate(wins, n)} · defeat {Stats.Rate(defeats, n)} · timeout {Stats.Rate(timeouts, n)}");
        for (int a = 0; a < config.Acts; a++)
        {
            var reached = records.Where(r => r.Acts[a].Reached).ToList();
            if (reached.Count == 0)
            {
                break;
            }
            var wipes = reached.Where(r => r.Acts[a].WipeSecond >= 0).Select(r => (double)r.Acts[a].WipeSecond).ToList();
            string wipeTimes = wipes.Count == 0
                ? "—"
                : $"{Sec(Stats.Quantile(wipes, 0.25))}/{Sec(Stats.Quantile(wipes, 0.5))}/{Sec(Stats.Quantile(wipes, 0.75))}";
            sb.AppendLine($"act {a + 1}: reached {Stats.Rate(reached.Count, n)} · cleared {Stats.Rate(reached.Count(r => r.Acts[a].Cleared), n)} · wipe time p25/p50/p75 {wipeTimes} (n={wipes.Count})");
            sb.AppendLine($"       p50: kills {Median(reached, r => r.Acts[a].Kills)} · elites {Median(reached, r => r.Acts[a].EliteKills)} · level {Median(reached, r => r.Acts[a].LevelAtEnd)} · damage taken {Median(reached, r => r.Acts[a].DamageTaken)} · hero deaths {Median(reached, r => r.Acts[a].HeroDeaths)} · swaps {Median(reached, r => r.Acts[a].Swaps)} · dashes {Median(reached, r => r.Acts[a].Dashes)} · assists {Median(reached, r => r.Acts[a].Assists)} · duets {Median(reached, r => r.Acts[a].Duets)} · shrines {Median(reached, r => r.Acts[a].Shrines)} · altars {Median(reached, r => r.Acts[a].Altars)} · peak enemies {Median(reached, r => r.Acts[a].PeakEnemies)}");
        }
        var byMinute = records.Where(r => r.Outcome == "defeat")
            .GroupBy(r => r.Ticks / (60 * SimConstants.TicksPerSecond))
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}:{g.Count()}");
        sb.AppendLine($"defeats by world minute: {string.Join(" ", byMinute)}");
        var killers = records.Where(r => r.Outcome == "defeat")
            .GroupBy(r => r.Killer)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}:{g.Count()}");
        sb.AppendLine($"defeats by killer: {string.Join(" ", killers)}");
        return sb.ToString();
    }

    private static string Sec(double seconds) => RunSetup.Clock((int)seconds * SimConstants.TicksPerSecond);

    private static string Median(List<RunRecord> records, Func<RunRecord, long> selector) =>
        Stats.Quantile(records.Select(r => (double)selector(r)).ToList(), 0.5).ToString("0");
}
