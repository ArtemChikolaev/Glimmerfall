using System;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Determinism;

namespace Glimmerfall.Sim.Cli;

/// <summary>Общая сборка конфига, сидов и политики из аргументов — одна на все команды.</summary>
public static class RunSetup
{
    public static SimConfig BuildConfig(Args args, string defaultScenario = "run", int defaultActs = 1, int defaultActSeconds = 480)
    {
        string scenario = args.Get("scenario", defaultScenario);
        SimConfig config = scenario switch
        {
            "run" => SimConfig.Run(args.GetInt("acts", defaultActs)),
            "horde" => SimConfig.HordeBench(args.GetInt("enemies", 5000), args.GetInt("projectiles", 2000)),
            _ => throw new ArgumentException($"unknown scenario '{scenario}' (run|horde)"),
        };
        if (config.Scenario == Scenario.Run)
        {
            config.ActTicks = args.GetInt("act-seconds", defaultActSeconds) * SimConstants.TicksPerSecond;
            config.BossLeadTicks = Math.Min(config.BossLeadTicks, config.ActTicks / 4);
            config.EliteFirstTicks = Math.Min(config.EliteFirstTicks, config.ActTicks / 3);
            config.ElitePeriodTicks = Math.Min(config.ElitePeriodTicks, config.ActTicks / 3);
        }
        config.Tier = args.GetInt("tier", 0);
        string squad = args.Get("squad", "");
        if (squad.Length > 0)
        {
            string[] names = squad.Split(',');
            if (names.Length != 3)
            {
                throw new ArgumentException("--squad needs three hero names");
            }
            for (int i = 0; i < 3; i++)
            {
                int id = Array.FindIndex(config.HeroKinds, k => k.Name == names[i]);
                if (id < 0)
                {
                    throw new ArgumentException($"unknown hero '{names[i]}'");
                }
                config.Squad[i] = id;
            }
        }
        string meta = args.Get("meta", "");
        if (meta.Length > 0)
        {
            string[] ranks = meta.Split(',');
            config.Meta = new MetaLoadout
            {
                Damage = byte.Parse(ranks[0]),
                Health = byte.Parse(ranks[1]),
                Speed = byte.Parse(ranks[2]),
                Magnet = byte.Parse(ranks[3]),
            };
        }
        foreach (string assignment in args.GetAll("set"))
        {
            config.ApplyOverride(assignment);
        }
        return config;
    }

    public static IBotPolicy BuildPolicy(string name)
    {
        if (name == "kiter")
        {
            return new KiterPolicy();
        }
        if (name == "afk")
        {
            return new AfkPolicy();
        }
        if (name.StartsWith("afk-after:", StringComparison.Ordinal))
        {
            return new AfkAfterPolicy(new KiterPolicy(), int.Parse(name.Substring("afk-after:".Length)));
        }
        throw new ArgumentException($"unknown policy '{name}' (kiter|afk|afk-after:SECONDS)");
    }

    public static ulong RunSeed(string baseSeed, int index) => SeedHash.FromString(baseSeed + "-" + index);

    public static string Describe(SimConfig config)
    {
        string squad = string.Join(",", Array.ConvertAll(config.Squad, id => config.HeroKinds[id].Name));
        string overrides = config.Overrides.Count == 0 ? "none" : string.Join(" ", config.Overrides);
        return config.Scenario == Scenario.HordeBench
            ? $"scenario horde · enemies {config.BenchEnemies} · projectiles {config.BenchProjectiles} · squad {squad}"
            : $"acts {config.Acts} × {config.ActTicks / SimConstants.TicksPerSecond} s · tier {config.Tier} · squad {squad} · meta {config.Meta} · overrides: {overrides}";
    }

    public static string Clock(int ticks)
    {
        int seconds = ticks / SimConstants.TicksPerSecond;
        return $"{seconds / 60}:{seconds % 60:00}";
    }

    public static string EnemyName(SimConfig config, int kind) =>
        kind >= 0 && kind < config.EnemyKinds.Length ? config.EnemyKinds[kind].Name : "-";
}
