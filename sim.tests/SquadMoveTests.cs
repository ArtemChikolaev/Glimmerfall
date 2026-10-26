using System.Linq;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Numerics;
using Xunit;

namespace Glimmerfall.Sim.Tests;

/// <summary>Приёмы отряда (PRD §5): ассист резервного героя, удар входящего при смене, связка и дуэт-приёмы.</summary>
public class SquadMoveTests
{
    private static World QuietWorld(string seed, int[]? squad = null)
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("spawn.startMilli=0");
        config.ApplyOverride("spawn.endMilli=0");
        config.ApplyOverride("elite.firstSeconds=1000");
        if (squad != null)
        {
            config.Squad = squad;
        }
        var world = new World(config, SeedHash.FromString(seed)) { CollectEvents = true };
        world.Step(default);
        return world;
    }

    /// <summary>Ассист, затем свап на того же героя — связка; события — только последнего шага (свапа).</summary>
    private static void Link(World world)
    {
        world.Step(Press(InputButtons.Assist));
        world.Step(Press(InputButtons.Swap));
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

    [Fact]
    public void CleaveHitsTheArcInFrontNearestFirstUpToTheCap()
    {
        World world = QuietWorld("cleave-arc");
        HeroKind lamplighter = world.ActiveHero;
        Assert.Equal(AttackPattern.Cleave, lamplighter.Pattern);
        Fix x = world.Squad.X;
        Fix y = world.Squad.Y;
        // перед героем (вправо) — больше врагов, чем задевает удар; сзади и вне досягаемости — по одному
        int[] front = Enumerable.Range(0, lamplighter.Pierce + 2)
            .Select(k => world.SpawnEnemyForTest(EnemyIds.Brute, x + Fix.FromInt(20 + 4 * k), y + Fix.FromInt(k % 2 == 0 ? 6 : -6)))
            .ToArray();
        int behind = world.SpawnEnemyForTest(EnemyIds.Brute, x - Fix.FromInt(30), y);
        int far = world.SpawnEnemyForTest(EnemyIds.Brute, x + Fix.FromInt(120), y);
        int full = world.Enemies.Hp[behind];
        world.Squad.AttackCooldown[world.Squad.Active] = 0;
        world.Step(default);
        SimEvent attack = Assert.Single(world.Events, e => e.Type == SimEventType.HeroAttack);
        Assert.Equal(lamplighter.Pierce, attack.Amount);
        Assert.Equal(lamplighter.Pierce, front.Count(i => world.Enemies.Hp[i] < full));
        Assert.True(world.Enemies.Hp[front[0]] < full && world.Enemies.Hp[front[1]] < full, "ближние задеты первыми");
        Assert.Equal(full, world.Enemies.Hp[behind]);
        Assert.Equal(full, world.Enemies.Hp[far]);
    }

    [Fact]
    public void HeavyCleaveThrowsRankAndFileButNotElites()
    {
        World world = QuietWorld("cleave-heavy", new[] { HeroIds.Warden, HeroIds.Lamplighter, HeroIds.Sparkwright });
        Fix x = world.Squad.X;
        Fix y = world.Squad.Y;
        int brute = world.SpawnEnemyForTest(EnemyIds.Brute, x + Fix.FromInt(30), y);
        int elite = world.SpawnEnemyForTest(EnemyIds.Ravager, x + Fix.FromInt(40), y + Fix.FromInt(14));
        Fix bruteX = world.Enemies.X[brute];
        Fix eliteX = world.Enemies.X[elite];
        world.Enemies.Hp[brute] = 1000;
        world.Squad.AttackCooldown[world.Squad.Active] = 0;
        world.Step(default);
        Assert.Contains(world.Events, e => e.Type == SimEventType.HeroAttack);
        Assert.True(world.Enemies.X[brute] >= bruteX + world.ActiveHero.CleavePushPx - Fix.FromInt(2), "рядового отбросило");
        Assert.True(world.Enemies.X[elite] < eliteX + Fix.FromInt(4), "элита стоит");
    }

    [Fact]
    public void ArmorReducesDamageButNeverToZero()
    {
        HeroKind warden = SpikeContent.CreateHeroKinds()[HeroIds.Warden];
        Assert.Equal(Rules.Percent(10, 100 - warden.ArmorPercent), Rules.DamageTaken(warden, 10));
        Assert.Equal(1, Rules.DamageTaken(warden, 1));
        Assert.Equal(0, Rules.DamageTaken(warden, 0));
    }

    [Fact]
    public void AssistOpensTheLinkWindowAndItCloses()
    {
        World world = QuietWorld("link-window");
        world.Step(Press(InputButtons.Assist));
        Assert.Equal(Rules.LinkWindowTicks, world.Squad.LinkTicks);
        Assert.Equal(1, world.Squad.LinkHelper);
        for (int t = 0; t < Rules.LinkWindowTicks; t++)
        {
            world.Step(default);
        }
        Assert.Equal(0, world.Squad.LinkTicks);
        Assert.Equal(-1, world.Squad.LinkHelper);
        world.Step(Press(InputButtons.Swap));
        Assert.Contains(world.Events, e => e.Type == SimEventType.SwapStrike);
        Assert.DoesNotContain(world.Events, e => e.Type == SimEventType.Duet);
        Assert.Equal(0, world.Stats.Acts[0].Duets);
    }

    [Fact]
    public void SwapInTheWindowFiresThePairsDuetInsteadOfTheEntryStrike()
    {
        World world = QuietWorld("link-duet");
        Link(world);
        SimEvent duet = Assert.Single(world.Events, e => e.Type == SimEventType.Duet);
        Assert.Equal((int)DuetKind.SteamBurst, duet.Kind);
        Assert.Equal(HeroIds.Warden, duet.Source);
        Assert.Equal(HeroIds.Lamplighter, duet.Target);
        Assert.DoesNotContain(world.Events, e => e.Type == SimEventType.SwapStrike);
        Assert.Equal(1, world.Squad.Active);
        Assert.Equal(0, world.Squad.LinkTicks);
        Assert.Equal(1, world.Stats.Acts[0].Duets);
        Assert.Equal(1, world.Stats.Acts[0].Swaps);
    }

    [Theory]
    [InlineData(HeroIds.Lamplighter, HeroIds.Warden, DuetKind.SteamBurst)]
    [InlineData(HeroIds.Warden, HeroIds.Lamplighter, DuetKind.SteamBurst)]
    [InlineData(HeroIds.Warden, HeroIds.Sparkwright, DuetKind.IceStorm)]
    [InlineData(HeroIds.Sparkwright, HeroIds.Warden, DuetKind.IceStorm)]
    [InlineData(HeroIds.Sparkwright, HeroIds.Lamplighter, DuetKind.Firestorm)]
    [InlineData(HeroIds.Lamplighter, HeroIds.Sparkwright, DuetKind.Firestorm)]
    public void DuetIsChosenByThePairsElementsInAnyOrder(int active, int helper, DuetKind expected)
    {
        int third = 3 - active - helper;
        World world = QuietWorld($"link-pair-{active}-{helper}", new[] { active, helper, third });
        Link(world);
        Assert.Equal((int)expected, Assert.Single(world.Events, e => e.Type == SimEventType.Duet).Kind);
    }

    [Fact]
    public void ForcedSwapClosesTheWindowWithoutADuet()
    {
        World world = QuietWorld("link-forced");
        world.Step(Press(InputButtons.Assist));
        world.Squad.Hp[0] = 1;
        world.SpawnEnemyForTest(EnemyIds.Shambler, world.Squad.X + Fix.FromInt(3), world.Squad.Y);
        bool down = false;
        for (int t = 0; t < Rules.LinkWindowTicks && !down; t++)
        {
            world.Step(default);
            down = world.Events.Any(e => e.Type == SimEventType.HeroDown);
            Assert.DoesNotContain(world.Events, e => e.Type == SimEventType.Duet);
        }
        Assert.True(down, "герой пал в окне связки");
        Assert.Equal(1, world.Squad.Active);
        Assert.Equal(0, world.Squad.LinkTicks);
        Assert.Equal(0, world.Stats.Acts[0].Duets);
    }

    [Fact]
    public void SteamBurstHitsAroundAndThrowsRankAndFileFar()
    {
        World world = QuietWorld("duet-steam");
        world.Step(Press(InputButtons.Assist));
        int brute = world.SpawnEnemyForTest(EnemyIds.Brute, world.Squad.X + Fix.FromInt(120), world.Squad.Y);
        world.Enemies.Hp[brute] = 1000;   // дуэт сильнее громилы — нужен живой, чтобы увидеть отброс
        Fix before = world.Enemies.X[brute];
        int hp = world.Enemies.Hp[brute];
        world.Step(Press(InputButtons.Swap));
        Assert.True(world.Enemies.Hp[brute] < hp);
        Assert.True(world.Enemies.X[brute] > before + Fix.FromInt(90), "паровой взрыв отбрасывает дальше обычного входа");
    }

    [Fact]
    public void IceStormStrikesHeavyFirstThenNearestDistinctTargets()
    {
        World world = QuietWorld("duet-ice", new[] { HeroIds.Warden, HeroIds.Sparkwright, HeroIds.Lamplighter });
        world.Step(Press(InputButtons.Assist));
        Fix x = world.Squad.X;
        Fix y = world.Squad.Y;
        world.SpawnEnemyForTest(EnemyIds.Ravager, x - Fix.FromInt(230), y);
        for (int k = 0; k < Rules.IceStormTargets + 4; k++)
        {
            world.SpawnEnemyForTest(EnemyIds.Brute, x + Fix.FromInt(30 + 15 * k), y + Fix.FromInt(20));
        }
        world.Step(Press(InputButtons.Swap));
        SimEvent[] bolts = world.Events.Where(e => e.Type == SimEventType.DuetStrike).ToArray();
        Assert.Equal(Rules.IceStormTargets, bolts.Length);
        Assert.Equal(x - Fix.FromInt(230), bolts[0].X);
        Assert.Equal(bolts.Length, bolts.Select(b => (b.X.Raw, b.Y.Raw)).Distinct().Count());
        Assert.True(bolts[1].X < bolts[2].X, "после элиты — от ближних рядовых к дальним");
    }

    [Fact]
    public void FirestormBlastsStartNearestAndKeepTheirSpacing()
    {
        World world = QuietWorld("duet-fire", new[] { HeroIds.Sparkwright, HeroIds.Lamplighter, HeroIds.Warden });
        world.Step(Press(InputButtons.Assist));
        Fix x = world.Squad.X;
        Fix y = world.Squad.Y;
        for (int k = 0; k < 12; k++)
        {
            world.SpawnEnemyForTest(EnemyIds.Brute, x + Fix.FromInt(30 + 20 * k), y);
        }
        world.Step(Press(InputButtons.Swap));
        SimEvent[] blasts = world.Events.Where(e => e.Type == SimEventType.DuetStrike).ToArray();
        Assert.InRange(blasts.Length, 2, Rules.FirestormBlasts);
        Assert.Equal(x + Fix.FromInt(30), blasts[0].X);
        for (int b = 1; b < blasts.Length; b++)
        {
            Assert.True(blasts[b].X - blasts[b - 1].X >= Rules.FirestormSpacingPx, "центры взрывов не ближе шага");
            Assert.True(blasts[b].X - x <= Rules.FirestormRangePx, "взрывы — в досягаемости");
        }
    }
}
