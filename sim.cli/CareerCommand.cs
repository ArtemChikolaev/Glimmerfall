using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glimmerfall.Sim.Bots;

namespace Glimmerfall.Sim.Cli;

/// <summary>
/// Карьера (идея --career из sim_arcade): забеги подряд с переносом меты и подъёмом тира после победы. Лестница и темп
/// меты калибруются ею, а не забегами с заданным комплектом (скилл balance-harness).
/// Угли за забег: убийства/40 + 25 за закрытый акт + 50 за победу; тратятся на самый низкий ранг меты, цена 20·(ранг+1).
/// </summary>
public static class CareerCommand
{
    public static int Run(Args args)
    {
        SimConfig template = RunSetup.BuildConfig(args, defaultActs: 3);
        int runs = args.GetInt("runs", 30);
        string baseSeed = args.Get("seed", "career");
        string policyName = args.Get("policy", "kiter");
        int maxTier = args.GetInt("max-tier", 7);
        int maxTicks = template.Acts * 12 * 60 * SimConstants.TicksPerSecond;
        args.EnsureAllUsed();

        var watch = Stopwatch.StartNew();
        int tier = template.Tier;
        MetaLoadout meta = template.Meta;
        int embers = 0;
        var tierEntry = new Dictionary<int, (int Run, MetaLoadout Meta)> { [tier] = (1, meta) };
        var results = new List<(int Tier, bool Won, int ActsCleared)>();
        for (int i = 0; i < runs; i++)
        {
            SimConfig config = BotCommand.Clone(template);
            config.Tier = tier;
            config.Meta = meta;
            var world = new World(config, RunSetup.RunSeed(baseSeed, i));
            RunResult result = RunDriver.Run(world, RunSetup.BuildPolicy(policyName), maxTicks);
            bool won = result.Outcome == RunOutcome.Victory;
            results.Add((tier, won, result.ActsCleared));
            embers += result.Kills / 40 + 25 * result.ActsCleared + (won ? 50 : 0);
            meta = Spend(meta, ref embers);
            if (won && tier < maxTier)
            {
                tier++;
                if (!tierEntry.ContainsKey(tier))
                {
                    tierEntry[tier] = (i + 2, meta);
                }
            }
        }
        watch.Stop();

        Console.WriteLine($"glimmerfall-sim career · balance {SimVersion.Balance} · policy {policyName} · runs {runs} · seed {baseSeed}-* · {RunSetup.Describe(template)} · {watch.Elapsed.TotalSeconds:0.0} s");
        Console.WriteLine("tier  first run  runs  wins  win%                acts cleared p50   meta at entry");
        foreach (var group in results.GroupBy(r => r.Tier).OrderBy(g => g.Key))
        {
            int n = group.Count();
            int w = group.Count(r => r.Won);
            double acts = Stats.Quantile(group.Select(r => (double)r.ActsCleared).ToList(), 0.5);
            var entry = tierEntry[group.Key];
            Console.WriteLine($"{group.Key,4}  {entry.Run,9}  {n,4}  {w,4}  {Stats.Rate(w, n),-18}  {acts,16:0}   {entry.Meta}");
        }
        Console.WriteLine($"final tier {tier} · meta {meta} · unspent embers {embers}");
        return 0;
    }

    private static MetaLoadout Spend(MetaLoadout meta, ref int embers)
    {
        while (true)
        {
            int lowest = Math.Min(Math.Min(meta.Damage, meta.Health), Math.Min(meta.Speed, meta.Magnet));
            if (lowest >= MetaLoadout.MaxRank)
            {
                return meta;
            }
            int cost = 20 * (lowest + 1);
            if (embers < cost)
            {
                return meta;
            }
            embers -= cost;
            if (meta.Damage == lowest)
            {
                meta.Damage++;
            }
            else if (meta.Health == lowest)
            {
                meta.Health++;
            }
            else if (meta.Speed == lowest)
            {
                meta.Speed++;
            }
            else
            {
                meta.Magnet++;
            }
        }
    }
}
