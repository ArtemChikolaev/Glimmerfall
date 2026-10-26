using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Replays;

namespace Glimmerfall.Sim.Cli;

/// <summary>Вердикт валидатора: контракт с сервером (server/internal/validator).</summary>
public sealed class Verdict
{
    [JsonPropertyName("file")] public string File { get; set; } = "";
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = "";
    [JsonPropertyName("balance")] public string Balance { get; set; } = "";
    [JsonPropertyName("seed")] public string Seed { get; set; } = "";
    [JsonPropertyName("steps")] public int Steps { get; set; }
    [JsonPropertyName("ticks")] public int Ticks { get; set; }
    [JsonPropertyName("outcome")] public string Outcome { get; set; } = "";
    [JsonPropertyName("digest")] public string Digest { get; set; } = "";
    [JsonPropertyName("score")] public int Score { get; set; }
}

[JsonSerializable(typeof(Verdict))]
internal partial class VerdictJson : JsonSerializerContext
{
}

public static class ReplayCommands
{
    /// <summary>
    /// Записывает реплей бот-прогона (golden-реплеи sim.tests/golden пишутся этой командой).
    /// <c>--daily ГГГГ-ММ-ДД</c> — дейлик даты: канонический конфиг и сид из <see cref="DailyRules"/>.
    /// </summary>
    public static int Record(Args args)
    {
        string daily = args.Get("daily", "");
        SimConfig config = daily.Length > 0 ? DailyRules.Config(daily) : RunSetup.BuildConfig(args, defaultActs: 1, defaultActSeconds: 120);
        string policyName = args.Get("policy", "kiter");
        string seed = daily.Length > 0 ? DailyRules.SeedText(daily) : args.Get("seed", "record");
        int every = args.GetInt("every", 600);
        string output = args.Get("out", "");
        int defaultMaxTicks = config.Scenario == Scenario.HordeBench ? 600 : config.Acts * (config.ActTicks + 120 * SimConstants.TicksPerSecond);
        int maxTicks = args.GetInt("max-ticks", defaultMaxTicks);
        args.EnsureAllUsed();
        if (output.Length == 0)
        {
            throw new ArgumentException("--out FILE is required");
        }
        var world = new World(config, SeedHash.FromString(seed));
        var recorder = new ReplayRecorder(world, policyName, every);
        RunDriver.Run(world, RunSetup.BuildPolicy(policyName), maxTicks, recorder);
        Replay replay = recorder.Finish();
        string text = "# seed text: " + seed + "\n" + replay.ToText();
        File.WriteAllText(output, text);
        Console.WriteLine($"recorded {output}: {replay.FinalSteps} steps, {replay.FinalTicks} ticks, {replay.Outcome}, {replay.Inputs.Count} input changes, digest {Replay.Hex(replay.FinalDigest)}");
        return 0;
    }

    /// <summary>
    /// Ре-симулирует реплеи и сверяет дайджесты; код выхода 1 при любом расхождении или отказе. «-» читает реплей из stdin.
    /// <c>--json</c> — машинный вердикт на файл одной строкой (так его читает серверный валидатор, G1.7).
    /// </summary>
    public static int Verify(Args args)
    {
        bool json = args.Has("json");
        string dailyDate = args.Get("daily", "");
        args.EnsureAllUsed();
        if (args.Positional.Count == 0)
        {
            throw new ArgumentException("verify needs replay files (or - for stdin)");
        }
        int failures = 0;
        foreach (string path in args.Positional)
        {
            var verdict = new Verdict { File = path };
            try
            {
                string text = path == "-" ? Console.In.ReadToEnd() : File.ReadAllText(path);
                Replay replay = Replay.Parse(text);
                verdict.Balance = replay.Balance;
                verdict.Seed = replay.Seed.ToString(CultureInfo.InvariantCulture);
                if (dailyDate.Length > 0 && !IsDaily(replay, dailyDate, out string reason))
                {
                    verdict.Message = reason;
                    failures++;
                    Console.WriteLine(json ? JsonSerializer.Serialize(verdict, VerdictJson.Default.Verdict) : $"FAIL {path}: {reason}");
                    continue;
                }
                World world = ReplayPlayer.Play(replay, out VerifyResult result);
                verdict.Ok = result.Ok;
                verdict.Message = result.Message;
                verdict.Steps = world.StepCount;
                verdict.Ticks = world.Tick;
                verdict.Outcome = world.Outcome.ToString();
                verdict.Digest = Replay.Hex(world.ComputeDigest());
                verdict.Score = result.Ok ? Rules.RunScore(world) : 0;
            }
            catch (Exception e) when (e is ReplayIncompatibleException or FormatException or IOException)
            {
                verdict.Ok = false;
                verdict.Message = e.Message;
            }
            if (!verdict.Ok)
            {
                failures++;
            }
            Console.WriteLine(json
                ? JsonSerializer.Serialize(verdict, VerdictJson.Default.Verdict)
                : $"{(verdict.Ok ? "OK  " : "FAIL")} {path}: {verdict.Message}");
        }
        return failures == 0 ? 0 : 1;
    }

    /// <summary>Реплей — дейлик даты: сид дня и отпечаток канонического конфига (отряд, тир, мета, акты, без оверрайдов).</summary>
    private static bool IsDaily(Replay replay, string date, out string reason)
    {
        if (replay.Seed != DailyRules.Seed(date))
        {
            reason = $"not the daily of {date}: seed {replay.Seed}";
            return false;
        }
        ulong expected = DailyRules.Config(date).ComputeFingerprint();
        if (replay.ConfigFingerprint != expected)
        {
            reason = $"not the daily of {date}: config fingerprint {Replay.Hex(replay.ConfigFingerprint)} != {Replay.Hex(expected)}";
            return false;
        }
        reason = "";
        return true;
    }
}
