using System;
using Glimmerfall.Sim.Input;

namespace Glimmerfall.Sim.Replays;

/// <summary>
/// Пишет реплей прогона: кадр ввода — только при изменении (ключ — номер шага), дайджест — каждые
/// <c>checkpointEvery</c> шагов. Создаётся до первого шага мира.
/// </summary>
public sealed class ReplayRecorder
{
    private readonly World _world;
    private readonly Replay _replay;
    private readonly int _checkpointEvery;
    private InputFrame _last;
    private bool _any;

    public ReplayRecorder(World world, string policy, int checkpointEvery = 600)
    {
        if (world.StepCount != 0)
        {
            throw new InvalidOperationException("recorder must be attached before the first step");
        }
        if (checkpointEvery <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(checkpointEvery));
        }
        _world = world;
        _checkpointEvery = checkpointEvery;
        _replay = new Replay
        {
            Seed = world.Seed,
            Policy = policy,
            ConfigHeader = world.Config.ToHeader(),
            ConfigFingerprint = world.Config.ComputeFingerprint(),
            CheckpointEvery = checkpointEvery,
        };
        _replay.Overrides.AddRange(world.Config.Overrides);
    }

    /// <summary>Вызывается перед <see cref="World.Step"/> с тем же кадром.</summary>
    public void Record(in InputFrame frame)
    {
        if (_any && frame == _last)
        {
            return;
        }
        _replay.Inputs.Add(new InputEntry(_world.StepCount, frame));
        _last = frame;
        _any = true;
    }

    /// <summary>Вызывается после <see cref="World.Step"/>.</summary>
    public void AfterStep()
    {
        if (_world.StepCount % _checkpointEvery == 0)
        {
            _replay.Checkpoints.Add(new Checkpoint(_world.StepCount, _world.ComputeDigest()));
        }
    }

    public Replay Finish()
    {
        _replay.FinalSteps = _world.StepCount;
        _replay.FinalTicks = _world.Tick;
        _replay.Outcome = _world.Outcome;
        _replay.FinalDigest = _world.ComputeDigest();
        return _replay;
    }
}
