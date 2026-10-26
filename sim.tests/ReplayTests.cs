using System;
using System.Collections.Generic;
using System.Globalization;
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

    /// <summary>Тот же вектор проверяет Go-сервер (server/internal/daily): сид дня одинаков в обоих языках.</summary>
    [Fact]
    public void DailyRulesAreCanonical()
    {
        Assert.Equal(4973729189407420658UL, DailyRules.Seed("2026-10-26"));
        SimConfig config = DailyRules.Config("2026-10-26");
        Assert.Equal(Scenario.Run, config.Scenario);
        Assert.Equal(0, config.Tier);
        Assert.Equal(default, config.Meta);
        Assert.Empty(config.Overrides);
        Assert.Equal(new[] { 0, 1, 2 }, config.Squad.OrderBy(id => id));
        Assert.Equal(config.ComputeFingerprint(), DailyRules.Config("2026-10-26").ComputeFingerprint());
        SimConfig easier = DailyRules.Config("2026-10-26");
        easier.ApplyOverride("spawn.max=1");
        Assert.NotEqual(config.ComputeFingerprint(), easier.ComputeFingerprint());
    }

    private static Replay RecordDaily(string date)
    {
        var world = new World(DailyRules.Config(date), DailyRules.Seed(date));
        var recorder = new ReplayRecorder(world, "afk", 600);
        RunDriver.Run(world, new AfkPolicy(), 300 * TestRuns.Second, recorder);
        return recorder.Finish();
    }

    /// <summary>
    /// Ревью 2026-10-26: 64-битный отпечаток подбирается через оверрайды за секунды, и «дейлик» с боссом на 2 HP проходил.
    /// Дейлик проверяется по каноническому конфигу дня, а не по отпечатку — даже идеальная коллизия не помогает.
    /// </summary>
    [Fact]
    public void ForgedDailyIsRejectedEvenWithAMatchingFingerprint()
    {
        Replay honest = RecordDaily("2026-10-26");
        Assert.NotNull(ReplayPlayer.PlayDaily(Replay.Parse(honest.ToText()), "2026-10-26", out VerifyResult ok));
        Assert.True(ok.Ok, ok.Message);
        Assert.Null(ReplayPlayer.PlayDaily(Replay.Parse(honest.ToText()), "2026-10-27", out VerifyResult otherDay));
        Assert.False(otherDay.Ok);

        Replay withOverride = Replay.Parse(honest.ToText());
        withOverride.Overrides.Add("enemy.gloom_warden.hp=2");
        Assert.Null(ReplayPlayer.PlayDaily(withOverride, "2026-10-26", out VerifyResult forged));
        Assert.False(forged.Ok);
        Assert.Equal(honest.ConfigFingerprint, withOverride.ConfigFingerprint);

        Replay benchHeader = Replay.Parse(honest.ToText().Replace("config scenario=0 ", "config scenario=1 "));
        Assert.Null(ReplayPlayer.PlayDaily(benchHeader, "2026-10-26", out VerifyResult bench));
        Assert.False(bench.Ok);
    }

    /// <summary>Реплей — чужой ввод: любой брак — FormatException или явный отказ, но не падение валидатора.</summary>
    [Fact]
    public void MalformedReplaysFailAsFormatErrors()
    {
        string text = TestRuns.Record(TestRuns.ShortAct(), "malformed", new KiterPolicy(), 30 * TestRuns.Second).ToText();
        string[] lines = text.Split('\n');
        string digestLine = Array.Find(lines, l => l.StartsWith("digest ", StringComparison.Ordinal))!;
        string[] digestLines = Array.FindAll(lines, l => l.StartsWith("digest ", StringComparison.Ordinal));
        var mutations = new[]
        {
            text.Replace(digestLine, "digest 300"),
            text.Replace("checkpoint-every 300", "checkpoint-every 99999999999"),
            text.Replace("checkpoint-every 300", "checkpoint-every 0"),
            text.Replace(" outcome=", " outcome=Bogus x="),
            text.Replace("final steps=", "final steps"),
            text.Replace("config scenario=0 acts=1 ", "config scenario=0 acts=0 "),
            text.Replace("actTicks=7200", "actTicks=0"),
            text.Replace("squad=0,1,2", "squad=9,9,9"),
            text.Replace(digestLines[0], digestLines[1]).Replace(digestLines[1] + "\n", digestLines[0] + "\n"),
            text + text.Split('\n').First(l => l.StartsWith("inputs ", StringComparison.Ordinal)) + "\n",
            text.Replace("override enemy.gloom_warden.hp=300", "override enemy.gloom_warden.hp=lots"),
        };
        foreach (string mutated in mutations)
        {
            Assert.NotEqual(text, mutated);
            Exception? error = Record.Exception(() => ReplayPlayer.Verify(Replay.Parse(mutated)));
            Assert.True(error is FormatException or ReplayIncompatibleException,
                $"expected a format error, got {error?.GetType().Name ?? "none"}: {error?.Message}\n{mutated.Substring(0, Math.Min(200, mutated.Length))}");
        }
    }

    /// <summary>Заголовок не зависит от культуры хоста: минус U+2212 шведской локали сломал бы реплей (ревью 2026-10-26).</summary>
    [Fact]
    public void ConfigHeaderIsCultureInvariant()
    {
        CultureInfo saved = CultureInfo.CurrentCulture;
        var odd = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        odd.NumberFormat.NegativeSign = "−";
        try
        {
            CultureInfo.CurrentCulture = odd;
            SimConfig config = TestRuns.ShortAct();
            config.ApplyOverride("spawn.perActPercent=-10");
            string header = config.ToHeader();
            Assert.Contains("spawnPerActPercent=-10", header);
            Assert.Equal(config.ComputeFingerprint(), SimConfig.FromHeader(header, config.Overrides).ComputeFingerprint());
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
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
