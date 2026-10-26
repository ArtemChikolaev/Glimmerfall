using Glimmerfall.Sim.Input;

namespace Glimmerfall.Sim.Replays;

public readonly struct VerifyResult
{
    public readonly bool Ok;
    /// <summary>Шаг первой расходящейся контрольной точки или −1.</summary>
    public readonly int MismatchStep;
    public readonly string Message;

    public VerifyResult(bool ok, int mismatchStep, string message)
    {
        Ok = ok;
        MismatchStep = mismatchStep;
        Message = message;
    }
}

/// <summary>
/// Ре-симуляция реплея и сверка дайджестов — то, что делают golden-тесты, CLI <c>verify</c> и серверный валидатор.
/// </summary>
public static class ReplayPlayer
{
    /// <summary>Воспроизводит реплей по его собственному конфигу (реплей отказывается, если данные изменились).</summary>
    public static World Play(Replay replay, out VerifyResult result) => Play(replay, replay.BuildConfig(), out result);

    /// <summary>
    /// Дейлик даты: правила берутся из <see cref="DailyRules"/>, а не из реплея. Его заголовок и оверрайды обязаны
    /// совпасть с каноническими дословно — 64-битный отпечаток не криптографический, и подобрать коллизию через
    /// оверрайды можно за секунды (находка ревью 2026-10-26). Возвращает null, если реплей не дейлик этой даты.
    /// </summary>
    public static World? PlayDaily(Replay replay, string date, out VerifyResult result)
    {
        SimConfig canonical = DailyRules.Config(date);
        if (replay.Seed != DailyRules.Seed(date))
        {
            result = new VerifyResult(false, -1, $"not the daily of {date}: seed {replay.Seed}");
            return null;
        }
        if (replay.Overrides.Count != 0 || replay.ConfigHeader != canonical.ToHeader())
        {
            result = new VerifyResult(false, -1, $"not the daily of {date}: config differs from the canonical daily");
            return null;
        }
        return Play(replay, canonical, out result);
    }

    public static World Play(Replay replay, SimConfig config, out VerifyResult result)
    {
        if (replay.Balance != SimVersion.Balance)
        {
            throw new ReplayIncompatibleException($"replay balance {replay.Balance} != sim {SimVersion.Balance}");
        }
        var world = new World(config, replay.Seed);
        int next = 0;
        int checkpoint = 0;
        InputFrame current = default;
        for (int step = 0; step < replay.FinalSteps; step++)
        {
            while (next < replay.Inputs.Count && replay.Inputs[next].Step == step)
            {
                current = replay.Inputs[next].Frame;
                next++;
            }
            world.Step(current);
            if (checkpoint < replay.Checkpoints.Count && replay.Checkpoints[checkpoint].Step == world.StepCount)
            {
                ulong digest = world.ComputeDigest();
                ulong expected = replay.Checkpoints[checkpoint].Digest;
                if (digest != expected)
                {
                    result = new VerifyResult(false, world.StepCount,
                        $"digest mismatch at step {world.StepCount} (tick {world.Tick}): expected {Replay.Hex(expected)}, got {Replay.Hex(digest)}");
                    return world;
                }
                checkpoint++;
            }
        }
        ulong final = world.ComputeDigest();
        if (world.Tick != replay.FinalTicks || world.Outcome != replay.Outcome || final != replay.FinalDigest)
        {
            result = new VerifyResult(false, world.StepCount,
                $"final state mismatch: ticks {world.Tick}/{replay.FinalTicks}, outcome {world.Outcome}/{replay.Outcome}, digest {Replay.Hex(final)}/{Replay.Hex(replay.FinalDigest)}");
            return world;
        }
        result = new VerifyResult(true, -1, $"ok: {replay.FinalSteps} steps, {world.Tick} ticks, {world.Outcome}, digest {Replay.Hex(final)}");
        return world;
    }

    public static VerifyResult Verify(Replay replay)
    {
        Play(replay, out VerifyResult result);
        return result;
    }
}
