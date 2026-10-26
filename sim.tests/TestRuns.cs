using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Replays;

namespace Glimmerfall.Sim.Tests;

/// <summary>
/// Короткие конфиги для тестов: акт на 2 минуты с элитой и боссом, чтобы цикл «акт с боссом» проходил быстро.
/// Короткий акт сжимает рост спавна, поэтому босс и конечный темп ослаблены — те же оверрайды стоят в командах
/// записи golden (golden/README.md).
/// </summary>
internal static class TestRuns
{
    public const int Second = SimConstants.TicksPerSecond;

    public static readonly string[] ShortActOverrides = { "enemy.gloom_warden.hp=300", "spawn.endMilli=4000" };

    public static SimConfig ShortAct(int acts = 1)
    {
        SimConfig config = SimConfig.Run(acts);
        config.ActTicks = 120 * Second;
        config.BossLeadTicks = 30 * Second;
        config.EliteFirstTicks = 40 * Second;
        config.ElitePeriodTicks = 40 * Second;
        foreach (string assignment in ShortActOverrides)
        {
            config.ApplyOverride(assignment);
        }
        return config;
    }

    public static Replay Record(SimConfig config, string seed, IBotPolicy policy, int maxTicks, int every = 300)
    {
        var world = new World(config, SeedHash.FromString(seed));
        var recorder = new ReplayRecorder(world, policy.Name, every);
        RunDriver.Run(world, policy, maxTicks, recorder);
        return recorder.Finish();
    }
}
