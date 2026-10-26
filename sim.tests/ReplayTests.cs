using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Replays;
using Xunit;

namespace Glimmerfall.Sim.Tests;

public class ReplayTests
{
    [Fact]
    public void SameSeedAndInputsGiveSameDigestsEveryTick()
    {
        List<ulong> a = Trace("same", new KiterPolicy());
        List<ulong> b = Trace("same", new KiterPolicy());
        Assert.Equal(a, b);
        Assert.NotEqual(a, Trace("other", new KiterPolicy()));
    }

    private static List<ulong> Trace(string seed, IBotPolicy policy)
    {
        var world = new World(TestRuns.ShortAct(), SeedHash.FromString(seed));
        var digests = new List<ulong>();
        for (int i = 0; i < 40 * TestRuns.Second; i++)
        {
            world.Step(policy.Decide(world));
            digests.Add(world.ComputeDigest());
        }
        return digests;
    }

    [Fact]
    public void RecordedRunReplaysBitExactThroughText()
    {
        Replay replay = TestRuns.Record(TestRuns.ShortAct(), "roundtrip", new KiterPolicy(), 160 * TestRuns.Second);
        Assert.True(replay.Inputs.Count > 10);
        Assert.Contains(replay.Inputs, entry => entry.Frame.Pick != 0);
        Replay parsed = Replay.Parse(replay.ToText());
        Assert.Equal(replay.ToText(), parsed.ToText());
        VerifyResult result = ReplayPlayer.Verify(parsed);
        Assert.True(result.Ok, result.Message);
    }

    [Fact]
    public void TamperedInputIsCaughtAtACheckpoint()
    {
        Replay replay = TestRuns.Record(TestRuns.ShortAct(), "tamper", new KiterPolicy(), 60 * TestRuns.Second);
        int index = replay.Inputs.FindIndex(entry => entry.Step > 600 && entry.Frame.MoveX != 0);
        Assert.True(index >= 0);
        InputEntry original = replay.Inputs[index];
        InputFrame changed = original.Frame;
        changed.MoveX = (sbyte)-changed.MoveX;
        replay.Inputs[index] = new InputEntry(original.Step, changed);
        VerifyResult result = ReplayPlayer.Verify(replay);
        Assert.False(result.Ok);
        Assert.True(result.MismatchStep > original.Step, result.Message);
    }

    [Fact]
    public void ReplayFromAnotherBalanceIsRefused()
    {
        Replay replay = TestRuns.Record(TestRuns.ShortAct(), "balance", new AfkPolicy(), 10 * TestRuns.Second);
        replay.Balance = "0.0.0-old";
        Assert.Throws<ReplayIncompatibleException>(() => ReplayPlayer.Verify(replay));
    }

    [Fact]
    public void ChangedGameDataIsRefusedByFingerprint()
    {
        Replay replay = TestRuns.Record(TestRuns.ShortAct(), "fingerprint", new AfkPolicy(), 10 * TestRuns.Second);
        string text = replay.ToText().Replace("\nfingerprint ", "\noverride enemy.shambler.hp=99\nfingerprint ");
        Assert.Throws<ReplayIncompatibleException>(() => ReplayPlayer.Verify(Replay.Parse(text)));
    }

    [Fact]
    public void OverridesTravelWithTheReplay()
    {
        SimConfig config = TestRuns.ShortAct();
        config.ApplyOverride("enemy.runner.speed=70");
        Replay replay = TestRuns.Record(config, "override", new KiterPolicy(), 50 * TestRuns.Second);
        Assert.Equal(TestRuns.ShortActOverrides.Append("enemy.runner.speed=70"), replay.Overrides);
        Assert.True(ReplayPlayer.Verify(Replay.Parse(replay.ToText())).Ok);
    }

    [Fact]
    public void UnknownOverrideKeyFailsLoudly()
    {
        Assert.Throws<ArgumentException>(() => TestRuns.ShortAct().ApplyOverride("enemy.ghost.hp=1"));
        Assert.Throws<ArgumentException>(() => TestRuns.ShortAct().ApplyOverride("spawn.rate=1"));
    }
}

/// <summary>
/// Golden-реплеи: записаны командой <c>glimmerfall-sim record</c> (см. golden/README.md) и обязаны воспроизводиться
/// бит-в-бит на Windows x64, Linux x64 и macOS arm64. Расхождение — сдвиг правил или RNG: перегенерация только
/// осознанная, с проверкой диффа дайджестов глазами (скилл sim-determinism).
/// </summary>
public class GoldenReplayTests
{
    public static IEnumerable<object[]> GoldenFiles() =>
        Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "golden"), "*.gfr")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new object[] { Path.GetFileName(path) });

    [Theory]
    [MemberData(nameof(GoldenFiles))]
    public void GoldenReplayIsBitExact(string file)
    {
        Replay replay = Replay.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "golden", file)));
        VerifyResult result = ReplayPlayer.Verify(replay);
        Assert.True(result.Ok, $"{file}: {result.Message}");
    }

    [Fact]
    public void GoldenSetIsPresent()
    {
        Assert.True(GoldenFiles().Count() >= 3, "expected at least 3 golden replays in sim.tests/golden");
    }
}
