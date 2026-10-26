using System.Linq;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Numerics;
using Xunit;

namespace Glimmerfall.Sim.Tests;

/// <summary>Инварианты сима (скилл sim-invariants): пулы, модалки, отряд, единые формулы.</summary>
public class InvariantTests
{
    [Fact]
    public void StaleHandleNeverResolvesToANewcomer()
    {
        var pool = new EnemyPool(4);
        int first = pool.Spawn(0, Fix.Zero, Fix.Zero, Fix.One, 10, 0);
        int handle = pool.Handle(first);
        pool.Alive[first] = false;
        pool.Release(first);
        int second = pool.Spawn(1, Fix.One, Fix.One, Fix.One, 10, 0);
        Assert.Equal(first, second);
        Assert.False(pool.IsLive(handle));
        Assert.True(pool.IsLive(pool.Handle(second)));
    }

    /// <summary>Хендл живой сущности остаётся живым и после 32 768 переиспользований слота (ревью 2026-10-26).</summary>
    [Fact]
    public void HandleStaysValidAtHighGenerations()
    {
        var pool = new EnemyPool(1);
        int first = pool.Spawn(0, Fix.Zero, Fix.Zero, Fix.One, 10, 0);
        int staleHandle = pool.Handle(first);
        for (int i = 0; i < 40_000; i++)
        {
            pool.Alive[0] = false;
            pool.Release(0);
            pool.Spawn(0, Fix.Zero, Fix.Zero, Fix.One, 10, 0);
        }
        Assert.True(pool.Handle(0) >= 0);
        Assert.True(pool.IsLive(pool.Handle(0)));
        Assert.False(pool.IsLive(staleHandle));
    }

    /// <summary>
    /// Одиночный враг вплотную к стоящему герою обязан бить: раньше преследователь вставал в 1 px от касания, а урон
    /// считался только при перекрытии, и ни рядовой, ни элита урона не наносили (ревью 2026-10-26).
    /// </summary>
    [Theory]
    [InlineData(EnemyIds.Brute)]
    [InlineData(EnemyIds.Ravager)]
    public void LoneChaserHitsAStandingHero(int kind)
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("spawn.startMilli=0");
        config.ApplyOverride("spawn.endMilli=0");
        config.ApplyOverride("elite.firstSeconds=1000");
        config.ApplyOverride("hero.lamplighter.damage=0");
        var world = new World(config, SeedHash.FromString("contact"));
        world.SpawnEnemyForTest(kind, world.Squad.X + Fix.FromInt(60), world.Squad.Y);
        var afk = new AfkPolicy();
        for (int i = 0; i < 10 * TestRuns.Second; i++)
        {
            world.Step(afk.Decide(world));
        }
        Assert.True(world.Stats.Acts[0].DamageTaken > 0, $"{config.EnemyKinds[kind].Name} never hit the standing hero");
    }

    [Fact]
    public void InvalidPickKeepsTheWorldPaused()
    {
        var world = new World(TestRuns.ShortAct(), SeedHash.FromString("modal"));
        var kiter = new KiterPolicy();
        int guard = 0;
        while (world.Modal == ModalKind.None && guard++ < 60 * TestRuns.Second)
        {
            world.Step(kiter.Decide(world));
        }
        Assert.Equal(ModalKind.Blessing, world.Modal);
        int tick = world.Tick;
        int steps = world.StepCount;
        world.Step(new InputFrame { Pick = 9 });
        world.Step(InputFrame.Move(127, 0));
        Assert.Equal(ModalKind.Blessing, world.Modal);
        Assert.Equal(tick, world.Tick);
        Assert.Equal(steps + 2, world.StepCount);
        world.Step(InputFrame.Choose(0));
        Assert.Equal(tick, world.Tick);
    }

    [Fact]
    public void DriverWatchdogCatchesAPolicyThatIgnoresModals()
    {
        var world = new World(TestRuns.ShortAct(), SeedHash.FromString("stall"));
        Assert.Throws<BotStallException>(() => RunDriver.Run(world, new IgnoresModals(), 120 * TestRuns.Second, stallSteps: 120));
    }

    private sealed class IgnoresModals : IBotPolicy
    {
        private readonly KiterPolicy _kiter = new();
        public string Name => "ignores-modals";
        public InputFrame Decide(World world) => world.Modal == ModalKind.None ? _kiter.Decide(world) : InputFrame.Move(127, 0);
    }

    [Fact]
    public void ActiveHeroDeathForcesASwapAndTheLastDeathEndsTheRun()
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("hero.lamplighter.hp=1");
        var world = new World(config, SeedHash.FromString("swap"));
        var afk = new AfkPolicy();
        int guard = 0;
        while (world.Squad.Alive[0] && guard++ < 120 * TestRuns.Second)
        {
            world.Step(afk.Decide(world));
        }
        Assert.False(world.Squad.Alive[0]);
        Assert.Equal(1, world.Squad.Active);
        Assert.True(world.Squad.InvulnerableTicks > 0);
        RunDriver.Run(world, afk, 600 * TestRuns.Second);
        Assert.Equal(RunOutcome.Defeat, world.Outcome);
        Assert.Equal(0, world.Squad.AliveCount);
        Assert.True(world.Stats.KillerKind >= 0);
    }

    [Fact]
    public void ActWithBossClosesThroughCheckpointIntoTheNextAct()
    {
        var world = new World(TestRuns.ShortAct(acts: 2), SeedHash.FromString("acts"));
        RunDriver.Run(world, new KiterPolicy(), 600 * TestRuns.Second);
        Assert.True(world.Stats.Acts[0].Cleared, "kiter should clear the short first act");
        Assert.True(world.Stats.Acts[1].Reached);
        Assert.True(world.Stats.Acts[1].StartTick > world.Stats.Acts[0].StartTick);
    }

    [Fact]
    public void HordeBenchHoldsTheRequestedCounts()
    {
        var world = new World(SimConfig.HordeBench(), SeedHash.FromString("horde"));
        var kiter = new KiterPolicy();
        for (int i = 0; i < 300; i++)
        {
            world.Step(kiter.Decide(world));
        }
        Assert.InRange(world.Enemies.Count, 4900, 5000);
        Assert.InRange(world.Projectiles.Count, 1900, 2000);
        Assert.Equal(RunOutcome.InProgress, world.Outcome);
    }

    [Fact]
    public void EventsAreRecordedOnlyWhenRequested()
    {
        var quiet = new World(SimConfig.HordeBench(), SeedHash.FromString("events"));
        var loud = new World(SimConfig.HordeBench(), SeedHash.FromString("events")) { CollectEvents = true };
        var kiter = new KiterPolicy();
        bool sawKill = false;
        for (int i = 0; i < 240; i++)
        {
            quiet.Step(kiter.Decide(quiet));
            loud.Step(kiter.Decide(loud));
            sawKill |= loud.Events.Any(e => e.Type == SimEventType.EnemyKilled);
            Assert.Equal(quiet.ComputeDigest(), loud.ComputeDigest());
        }
        Assert.Empty(quiet.Events);
        Assert.True(sawKill);
    }

    [Fact]
    public void RunContentIsACopyPerConfig()
    {
        SimConfig a = SimConfig.Run();
        SimConfig b = SimConfig.Run();
        a.ApplyOverride("enemy.shambler.hp=999");
        Assert.NotEqual(a.EnemyKinds[EnemyIds.Shambler].Hp, b.EnemyKinds[EnemyIds.Shambler].Hp);
        Assert.NotSame(a.HeroKinds[0], b.HeroKinds[0]);
    }

    [Fact]
    public void UiNumbersComeFromTheSameRules()
    {
        HeroKind lamplighter = SpikeContent.CreateHeroKinds()[HeroIds.Lamplighter];
        var meta = new MetaLoadout { Damage = 2 };
        Assert.Equal(lamplighter.Damage * (100 + 15 * 3 + 5 * 2) / 100, Rules.HeroDamage(lamplighter, 3, meta));
        Assert.Equal(6, Rules.AttackInterval(lamplighter, 50));
        Assert.Equal(lamplighter.ProjectileCount + 2, Rules.ProjectileCount(lamplighter, 2));
    }
}

/// <summary>Харнесс как гейт столпа «угроза» в форме смоук-теста; калиброванный гейт — BACKLOG G3.4.</summary>
public class BotHarnessTests
{
    [Fact]
    public void AfkSquadDiesBeforeTheEndOfTheFirstAct()
    {
        for (int i = 0; i < 4; i++)
        {
            SimConfig config = TestRuns.ShortAct();
            config.ActTicks = 180 * TestRuns.Second;
            var world = new World(config, SeedHash.FromString("afk-gate-" + i));
            RunDriver.Run(world, new AfkPolicy(), 300 * TestRuns.Second);
            Assert.Equal(RunOutcome.Defeat, world.Outcome);
            Assert.True(world.Tick < config.ActTicks, $"afk survived to tick {world.Tick}");
        }
    }

    [Fact]
    public void KiterOutlivesAfkOnTheSameSeeds()
    {
        for (int i = 0; i < 3; i++)
        {
            ulong seed = SeedHash.FromString("compare-" + i);
            var afk = new World(TestRuns.ShortAct(), seed);
            RunDriver.Run(afk, new AfkPolicy(), 300 * TestRuns.Second);
            var kiter = new World(TestRuns.ShortAct(), seed);
            RunDriver.Run(kiter, new KiterPolicy(), 300 * TestRuns.Second);
            Assert.True(kiter.Outcome == RunOutcome.Victory || kiter.Tick > afk.Tick, $"seed {i}: kiter {kiter.Tick} vs afk {afk.Tick}");
        }
    }
}
