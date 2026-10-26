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
    public static World Play(Replay replay, out VerifyResult result)
    {
        if (replay.Balance != SimVersion.Balance)
        {
            throw new ReplayIncompatibleException($"replay balance {replay.Balance} != sim {SimVersion.Balance}");
        }
        var world = new World(replay.BuildConfig(), replay.Seed);
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
