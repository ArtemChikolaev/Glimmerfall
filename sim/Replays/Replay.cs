using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Glimmerfall.Sim.Input;

namespace Glimmerfall.Sim.Replays;

public readonly struct InputEntry
{
    public readonly int Step;
    public readonly InputFrame Frame;

    public InputEntry(int step, InputFrame frame)
    {
        Step = step;
        Frame = frame;
    }
}

public readonly struct Checkpoint
{
    public readonly int Step;
    public readonly ulong Digest;

    public Checkpoint(int step, ulong digest)
    {
        Step = step;
        Digest = digest;
    }
}

/// <summary>
/// Реплей = версия баланса + сид + конфиг (заголовок, оверрайды, отпечаток данных) + лог ввода по шагам + дайджесты.
/// Текстовый формат: строки «ключ значение», лог ввода — base64url упакованных записей (дельта шага varint и 4 байта кадра).
/// Дайджесты лежат строками, поэтому дифф golden показывает, с какой контрольной точки разошлось состояние.
/// </summary>
public sealed class Replay
{
    public const string Magic = "glimmerfall-replay";
    public const int FormatVersion = 1;

    public string Balance = SimVersion.Balance;
    public ulong Seed;
    public string Policy = "";
    public string ConfigHeader = "";
    public readonly List<string> Overrides = new();
    public ulong ConfigFingerprint;
    public readonly List<InputEntry> Inputs = new();
    public int CheckpointEvery;
    public readonly List<Checkpoint> Checkpoints = new();
    public int FinalSteps;
    public int FinalTicks;
    public RunOutcome Outcome;
    public ulong FinalDigest;

    /// <summary>Конфиг забега из реплея. Отпечаток не совпал — данные изменились без бампа версии баланса: отказ.</summary>
    public SimConfig BuildConfig()
    {
        SimConfig config = SimConfig.FromHeader(ConfigHeader, Overrides);
        ulong fingerprint = config.ComputeFingerprint();
        if (fingerprint != ConfigFingerprint)
        {
            throw new ReplayIncompatibleException(
                $"config fingerprint {Hex(fingerprint)} != replay {Hex(ConfigFingerprint)}: game data changed — bump SimVersion.Balance and regenerate golden replays deliberately");
        }
        return config;
    }

    public string ToText()
    {
        var sb = new StringBuilder();
        sb.Append(Magic).Append(' ').Append(FormatVersion.ToString(CultureInfo.InvariantCulture)).Append('\n');
        sb.Append("balance ").Append(Balance).Append('\n');
        sb.Append("policy ").Append(Policy.Length == 0 ? "-" : Policy).Append('\n');
        sb.Append("seed ").Append(Seed.ToString(CultureInfo.InvariantCulture)).Append('\n');
        sb.Append("config ").Append(ConfigHeader).Append('\n');
        foreach (string assignment in Overrides)
        {
            sb.Append("override ").Append(assignment).Append('\n');
        }
        sb.Append("fingerprint ").Append(Hex(ConfigFingerprint)).Append('\n');
        sb.Append("inputs ").Append(Base64Url.Encode(PackInputs(Inputs))).Append('\n');
        sb.Append("checkpoint-every ").Append(CheckpointEvery.ToString(CultureInfo.InvariantCulture)).Append('\n');
        foreach (Checkpoint c in Checkpoints)
        {
            sb.Append("digest ").Append(c.Step.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(Hex(c.Digest)).Append('\n');
        }
        sb.Append("final ")
            .Append("steps=").Append(FinalSteps.ToString(CultureInfo.InvariantCulture))
            .Append(" ticks=").Append(FinalTicks.ToString(CultureInfo.InvariantCulture))
            .Append(" outcome=").Append(Outcome.ToString())
            .Append(" digest=").Append(Hex(FinalDigest))
            .Append('\n');
        return sb.ToString();
    }

    /// <summary>
    /// Разбор текста реплея. Реплей — чужой ввод (сервер принимает его от клиента), поэтому любое отклонение от формата —
    /// <see cref="FormatException"/>: неизвестная или повторная строка, кривое число, шаги ввода и контрольные точки
    /// не строго по возрастанию, контрольная точка за пределами реплея.
    /// </summary>
    public static Replay Parse(string text)
    {
        var replay = new Replay();
        bool sawMagic = false;
        bool sawFinal = false;
        bool sawInputs = false;
        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }
            int space = line.IndexOf(' ');
            string key = space < 0 ? line : line.Substring(0, space);
            string value = space < 0 ? "" : line.Substring(space + 1);
            switch (key)
            {
                case Magic:
                    if (ParseInt(value) != FormatVersion)
                    {
                        throw new ReplayIncompatibleException($"replay format {value} is not supported (expected {FormatVersion})");
                    }
                    sawMagic = true;
                    break;
                case "balance": replay.Balance = value; break;
                case "policy": replay.Policy = value == "-" ? "" : value; break;
                case "seed": replay.Seed = ParseULong(value); break;
                case "config": replay.ConfigHeader = value; break;
                case "override": replay.Overrides.Add(value); break;
                case "fingerprint": replay.ConfigFingerprint = ParseHex(value); break;
                case "inputs":
                    if (sawInputs)
                    {
                        throw new FormatException("duplicate inputs line");
                    }
                    UnpackInputs(Base64Url.Decode(value), replay.Inputs);
                    sawInputs = true;
                    break;
                case "checkpoint-every": replay.CheckpointEvery = ParseInt(value); break;
                case "digest":
                    string[] d = value.Split(' ');
                    if (d.Length != 2)
                    {
                        throw new FormatException($"bad digest line '{value}'");
                    }
                    replay.Checkpoints.Add(new Checkpoint(ParseInt(d[0]), ParseHex(d[1])));
                    break;
                case "final":
                    ParseFinal(value, replay);
                    sawFinal = true;
                    break;
                default:
                    throw new FormatException($"unknown replay line '{key}'");
            }
        }
        if (!sawMagic || !sawFinal || !sawInputs)
        {
            throw new FormatException("not a complete glimmerfall replay");
        }
        Validate(replay);
        return replay;
    }

    private static void ParseFinal(string value, Replay replay)
    {
        foreach (string token in value.Split(' '))
        {
            int eq = token.IndexOf('=');
            if (eq <= 0)
            {
                throw new FormatException($"bad final field '{token}'");
            }
            string k = token.Substring(0, eq);
            string v = token.Substring(eq + 1);
            switch (k)
            {
                case "steps": replay.FinalSteps = ParseInt(v); break;
                case "ticks": replay.FinalTicks = ParseInt(v); break;
                case "outcome":
                    if (!Enum.TryParse(v, ignoreCase: false, out RunOutcome outcome) || !Enum.IsDefined(typeof(RunOutcome), outcome))
                    {
                        throw new FormatException($"bad outcome '{v}'");
                    }
                    replay.Outcome = outcome;
                    break;
                case "digest": replay.FinalDigest = ParseHex(v); break;
                default: throw new FormatException($"unknown final field '{k}'");
            }
        }
    }

    /// <summary>Ввод и контрольные точки строго по возрастанию и внутри реплея — иначе проверка молча слабела бы.</summary>
    private static void Validate(Replay replay)
    {
        if (replay.FinalSteps < 0 || replay.FinalTicks < 0 || replay.FinalTicks > replay.FinalSteps)
        {
            throw new FormatException("bad final steps or ticks");
        }
        if (replay.CheckpointEvery <= 0)
        {
            throw new FormatException("checkpoint-every must be positive");
        }
        int last = -1;
        foreach (InputEntry entry in replay.Inputs)
        {
            if (entry.Step <= last || entry.Step >= replay.FinalSteps)
            {
                throw new FormatException($"input steps must increase and stay below {replay.FinalSteps}");
            }
            last = entry.Step;
        }
        last = 0;
        foreach (Checkpoint checkpoint in replay.Checkpoints)
        {
            if (checkpoint.Step <= last || checkpoint.Step > replay.FinalSteps)
            {
                throw new FormatException($"checkpoint steps must increase within 1..{replay.FinalSteps}");
            }
            last = checkpoint.Step;
        }
    }

    public static string Hex(ulong value) => value.ToString("x16", CultureInfo.InvariantCulture);

    private static ulong ParseHex(string value) =>
        ulong.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ulong result)
            ? result
            : throw new FormatException($"bad hex value '{value}'");

    private static ulong ParseULong(string value) =>
        ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong result)
            ? result
            : throw new FormatException($"bad number '{value}'");

    private static int ParseInt(string value) =>
        int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int result)
            ? result
            : throw new FormatException($"bad number '{value}'");

    private static byte[] PackInputs(List<InputEntry> inputs)
    {
        var bytes = new List<byte>(inputs.Count * 6);
        int last = 0;
        foreach (InputEntry entry in inputs)
        {
            uint delta = (uint)(entry.Step - last);
            last = entry.Step;
            while (delta >= 0x80)
            {
                bytes.Add((byte)(delta | 0x80));
                delta >>= 7;
            }
            bytes.Add((byte)delta);
            bytes.Add((byte)entry.Frame.MoveX);
            bytes.Add((byte)entry.Frame.MoveY);
            bytes.Add((byte)entry.Frame.Buttons);
            bytes.Add(entry.Frame.Pick);
        }
        return bytes.ToArray();
    }

    private static void UnpackInputs(byte[] bytes, List<InputEntry> output)
    {
        int i = 0;
        int step = 0;
        while (i < bytes.Length)
        {
            uint delta = 0;
            int shift = 0;
            byte b;
            do
            {
                if (i >= bytes.Length)
                {
                    throw new FormatException("truncated input log");
                }
                b = bytes[i++];
                if (shift > 28)
                {
                    throw new FormatException("input step delta overflows");
                }
                delta |= (uint)(b & 0x7F) << shift;
                shift += 7;
            }
            while ((b & 0x80) != 0);
            if (i + 4 > bytes.Length)
            {
                throw new FormatException("truncated input frame");
            }
            step += (int)delta;
            var frame = new InputFrame
            {
                MoveX = (sbyte)bytes[i],
                MoveY = (sbyte)bytes[i + 1],
                Buttons = (InputButtons)bytes[i + 2],
                Pick = bytes[i + 3],
            };
            i += 4;
            output.Add(new InputEntry(step, frame));
        }
    }
}

/// <summary>Реплей несовместим с текущим симом (формат, версия баланса или данные). Отказ явный, а не кривое воспроизведение.</summary>
public sealed class ReplayIncompatibleException : Exception
{
    public ReplayIncompatibleException(string message) : base(message)
    {
    }
}

internal static class Base64Url
{
    public static string Encode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static byte[] Decode(string text)
    {
        string padded = text.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}
