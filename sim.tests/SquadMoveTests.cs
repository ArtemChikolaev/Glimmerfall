using System.Linq;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Numerics;
using Xunit;

namespace Glimmerfall.Sim.Tests;

/// <summary>Приёмы отряда (PRD §5): ассист резервного героя и удар входящего при смене.</summary>
public class SquadMoveTests
{
    private static World QuietWorld(string seed)
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("spawn.startMilli=0");
        config.ApplyOverride("spawn.endMilli=0");
        config.ApplyOverride("elite.firstSeconds=1000");
        var world = new World(config, SeedHash.FromString(seed)) { CollectEvents = true };
        world.Step(default);
        return world;
    }

    private static InputFrame Press(InputButtons buttons) => new() { Buttons = buttons };

    [Fact]
    public void AssistFiresTheNextReservesSignatureAndGoesOnCooldown()
    {
        World world = QuietWorld("assist-ring");
        int near = world.SpawnEnemyForTest(EnemyIds.Shambler, world.Squad.X + Fix.FromInt(40), world.Squad.Y);
        int far = world.SpawnEnemyForTest(EnemyIds.Shambler, world.Squad.X + Fix.FromInt(300), world.Squad.Y);
        int hpNear = world.Enemies.Hp[near];
        int hpFar = world.Enemies.Hp[far];
        // резерв по кругу после фонарщика — страж (удар щитом по взгляду, взгляд отряда — вправо)
        world.Step(Press(InputButtons.Assist));
        Assert.Contains(world.Events, e => e.Type == SimEventType.Assist && e.Kind == HeroIds.Warden);
        Assert.True(!world.Enemies.Alive[near] || world.Enemies.Hp[near] < hpNear);
        Assert.Equal(hpFar, world.Enemies.Hp[far]);
        Assert.Equal(Rules.AssistCooldownTicks, world.Squad.AssistCooldown);
        Assert.Equal(0, world.Squad.Active);
        Assert.Equal(1, world.Stats.Acts[0].Assists);

        world.Step(Press(InputButtons.Assist));
        Assert.Equal(1, world.Stats.Acts[0].Assists);
    }

    [Fact]
    public void ShieldBashHitsOnlyTheFacingConeAndPushesRankAndFile()
    {
        World world = QuietWorld("assist-bash");
        Fix x = world.Squad.X;
        Fix y = world.Squad.Y;
        int ahead = world.SpawnEnemyForTest(EnemyIds.Brute, x + Fix.FromInt(50), y);
        int behind = world.SpawnEnemyForTest(EnemyIds.Brute, x - Fix.FromInt(50), y);
        Fix aheadX = world.Enemies.X[ahead];
        int behindHp = world.Enemies.Hp[behind];
        world.Step(Press(InputButtons.Assist));
        Assert.True(world.Enemies.X[ahead] > aheadX + Fix.FromInt(40), "рядового в конусе отбрасывает");
        Assert.Equal(behindHp, world.Enemies.Hp[behind]);
    }

    [Fact]
    public void ChainLightningHopsBetweenDistinctEnemiesNearestFirst()
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("spawn.startMilli=0");
        config.ApplyOverride("spawn.endMilli=0");
        config.ApplyOverride("elite.firstSeconds=1000");
        config.Squad = new[] { HeroIds.Lamplighter, HeroIds.Sparkwright, HeroIds.Warden };
        var world = new World(config, SeedHash.FromString("assist-chain")) { CollectEvents = true };
        world.Step(default);
        for (int k = 0; k < 10; k++)
        {
            world.SpawnEnemyForTest(EnemyIds.Brute, world.Squad.X + Fix.FromInt(60 + 70 * k), world.Squad.Y + Fix.FromInt(10));
        }
        world.Step(Press(InputButtons.Assist));
        SimEvent[] links = world.Events.Where(e => e.Type == SimEventType.ChainLink).ToArray();
        Assert.Equal(Rules.ChainLightningTargets, links.Length);
        Assert.Equal(Enumerable.Range(0, Rules.ChainLightningTargets), links.Select(e => e.Amount));
        int[] hit = world.Events.Where(e => e.Type is SimEventType.EnemyHit or SimEventType.EnemyKilled).Select(e => e.Target).ToArray();
        Assert.Equal(hit.Length, hit.Distinct().Count());
        Assert.True(links[0].DirX.Raw > 0 && links[0].DirX < Fix.FromInt(80), "первый прыжок — в ближайшего");
    }

    [Fact]
    public void AssistNeedsALivingReserve()
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("spawn.startMilli=0");
        var world = new World(config, SeedHash.FromString("assist-alone"));
        world.Squad.Alive[1] = false;
        world.Squad.Alive[2] = false;
        world.Step(Press(InputButtons.Assist));
        Assert.Equal(0, world.Squad.AssistCooldown);
        Assert.Equal(0, world.Stats.Acts[0].Assists);
    }

    [Fact]
    public void IncomingHeroStrikesOnSwap()
    {
        World world = QuietWorld("swap-strike");
        int close = world.SpawnEnemyForTest(EnemyIds.Brute, world.Squad.X + Fix.FromInt(30), world.Squad.Y);
        int hp = world.Enemies.Hp[close];
        world.Step(Press(InputButtons.Swap));
        Assert.Equal(1, world.Squad.Active);
        Assert.Contains(world.Events, e => e.Type == SimEventType.SwapStrike && e.Kind == HeroIds.Warden);
        Assert.True(world.Enemies.Hp[close] < hp);
    }
}
