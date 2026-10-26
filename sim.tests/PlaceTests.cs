using System.Linq;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;
using Xunit;

namespace Glimmerfall.Sim.Tests;

/// <summary>Места акта (PRD §4): святилища и алтари риска — расстановка от сида, постой, пробуждение и дар.</summary>
public class PlaceTests
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

    private static int Find(World world, PlaceKind kind) =>
        Enumerable.Range(0, world.Places.Count).First(i => world.Places.Kind[i] == kind);

    private static void StandAt(World world, int place)
    {
        world.Squad.X = world.Places.X[place];
        world.Squad.Y = world.Places.Y[place];
    }

    [Fact]
    public void PlacesAreLaidOutFromTheSeedAwayFromStartAndApart()
    {
        World a = QuietWorld("places-layout");
        World b = QuietWorld("places-layout");
        Assert.Equal(Rules.ShrinesPerAct + Rules.AltarsPerAct, a.Places.Count);
        Assert.Equal(Rules.ShrinesPerAct, Enumerable.Range(0, a.Places.Count).Count(i => a.Places.Kind[i] == PlaceKind.Shrine));
        long center = Fix.FromInt(SimConstants.ArenaSize / 2).Raw;
        for (int i = 0; i < a.Places.Count; i++)
        {
            Assert.Equal(a.Places.X[i], b.Places.X[i]);
            Assert.Equal(a.Places.Y[i], b.Places.Y[i]);
            long dx = a.Places.X[i].Raw - center;
            long dy = a.Places.Y[i].Raw - center;
            Assert.True(dx * dx + dy * dy >= Rules.PlaceKeepOutPx.Raw * Rules.PlaceKeepOutPx.Raw, "место вне круга старта");
            for (int j = 0; j < i; j++)
            {
                long px = a.Places.X[i].Raw - a.Places.X[j].Raw;
                long py = a.Places.Y[i].Raw - a.Places.Y[j].Raw;
                Assert.True(px * px + py * py >= Rules.PlaceSpacingPx.Raw * Rules.PlaceSpacingPx.Raw, "места не теснее шага");
            }
        }
    }

    [Fact]
    public void StandingInAShrineGrantsItsPowerOnceAndLeavingDrainsProgress()
    {
        World world = QuietWorld("places-shrine");
        int shrine = Find(world, PlaceKind.Shrine);
        StandAt(world, shrine);
        for (int t = 0; t < Rules.ShrineChannelTicks / 2; t++)
        {
            world.Step(default);
        }
        int half = world.Places.Progress[shrine];
        Assert.True(half > 0);
        world.Squad.X += Fix.FromInt(200);
        world.Step(default);
        Assert.True(world.Places.Progress[shrine] < half, "вне круга прогресс тает");

        StandAt(world, shrine);
        for (int t = 0; t < Rules.ShrineChannelTicks && world.Places.State[shrine] == PlaceState.Dormant; t++)
        {
            world.Step(default);
        }
        Assert.Equal(PlaceState.Spent, world.Places.State[shrine]);
        Assert.Contains(world.Events, e => e.Type == SimEventType.PlaceUsed);
        Assert.Equal(Rules.ShrineBuffTicks, world.Squad.ShrineTicks);
        Assert.Equal(1, world.Stats.Acts[0].Shrines);
        world.Step(default);
        Assert.Equal(Rules.ShrineBuffTicks - 1, world.Squad.ShrineTicks);
        Assert.Equal(14, Rules.ShrineDamage(10));
    }

    [Fact]
    public void TouchingAnAltarWakesItsGuardianAndKillingItGrantsABlessing()
    {
        World world = QuietWorld("places-altar");
        int altar = Find(world, PlaceKind.Altar);
        StandAt(world, altar);
        world.Step(default);
        Assert.Equal(PlaceState.Awake, world.Places.State[altar]);
        Assert.Contains(world.Events, e => e.Type == SimEventType.PlaceAwake);
        int guardian = world.Places.Guardian[altar];
        Assert.True(world.Enemies.IsLive(guardian), "страж появился в конце тика");
        Assert.Equal(EnemyIds.Ravager, world.Enemies.Kind[EnemyPool.SlotOf(guardian)]);
        Assert.Equal(1 + Rules.AltarEscort, world.Enemies.Count);

        world.KillEnemyForTest(EnemyPool.SlotOf(guardian));
        world.Step(default);
        world.Step(default);
        Assert.Equal(PlaceState.Spent, world.Places.State[altar]);
        Assert.Equal(1, world.Stats.Acts[0].Altars);
        Assert.Equal(ModalKind.Blessing, world.Modal);
    }
}
