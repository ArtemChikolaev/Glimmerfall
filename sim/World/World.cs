using System.Collections.Generic;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

public enum RunOutcome : byte
{
    InProgress = 0,
    Victory = 1,
    Defeat = 2,
}

public enum ModalKind : byte
{
    None = 0,
    /// <summary>Выбор благословения за уровень.</summary>
    Blessing = 1,
    /// <summary>Чекпоинт между актами.</summary>
    Checkpoint = 2,
}

/// <summary>
/// Состояние забега и его шаг. Один вызов <see cref="Step"/> — один шаг ввода. Пока открыта модалка, мир стоит:
/// шаг разрешает модалку, тик не растёт (лог ввода поэтому ключуется шагом, а не тиком). Порядок систем внутри тика
/// фиксирован и описан в <see cref="Step"/>.
/// </summary>
public sealed partial class World
{
    public const int EnemyCapacity = 8192;
    public const int ProjectileCapacity = 4096;
    public const int ShotCapacity = 1024;
    public const int GemCapacity = 4096;
    public const int MaxModalOptions = 3;

    /// <summary>Версия раскладки хеша состояния; меняется вместе с полями в <see cref="ComputeDigest"/>.
    /// 2 — радиусы снарядов и поколения всех слотов пулов (ревью 2026-10-26); 3 — откат ассиста отряда;
    /// 4 — окно связки и герой, открывший его; 5 — места акта и усиление святилища.</summary>
    public const int DigestLayoutVersion = 5;

    public readonly SimConfig Config;
    public readonly ulong Seed;
    public readonly RngSet Rng;
    public readonly Squad Squad = new();
    public readonly EnemyPool Enemies = new(EnemyCapacity);
    public readonly ProjectilePool Projectiles = new(ProjectileCapacity);
    public readonly ShotPool Shots = new(ShotCapacity);
    public readonly GemPool Gems = new(GemCapacity);
    public readonly PlacePool Places = new();
    public readonly SpatialGrid Grid = new(EnemyCapacity);
    public readonly RunStats Stats;
    public readonly int[] BlessingLevels = new int[Blessings.Count];

    /// <summary>События тика для презентации; пишутся, только если <see cref="CollectEvents"/> = true.</summary>
    public readonly List<SimEvent> Events = new();
    public bool CollectEvents;

    public int StepCount { get; private set; }
    public int Tick { get; private set; }
    public int ActIndex { get; private set; }
    /// <summary>Часы акта. Отдельны от тика мира (скилл sim-determinism, правило 3).</summary>
    public int ActTick { get; private set; }
    public RunOutcome Outcome { get; private set; }
    public ModalKind Modal { get; private set; }
    public int ModalOptionCount { get; private set; }
    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int PendingLevelUps { get; private set; }
    /// <summary>Хендл живого босса акта или −1.</summary>
    public int BossHandle { get; private set; } = -1;

    private readonly int[] _modalOptions = new int[MaxModalOptions];
    private readonly bool[] _heavyKind;
    private bool _bossSpawned;
    private bool _actCleared;
    private int _nextEliteTick;
    private int _spawnAccumulatorMilli;

    public World(SimConfig config, ulong seed)
    {
        Config = config;
        Seed = seed;
        Rng = new RngSet(seed);
        Stats = new RunStats(config.Acts);
        _heavyKind = new bool[config.EnemyKinds.Length];
        for (int k = 0; k < config.EnemyKinds.Length; k++)
        {
            _heavyKind[k] = config.EnemyKinds[k].Elite;
        }
        _separationX = new long[EnemyCapacity];
        _separationY = new long[EnemyCapacity];
        for (int i = 0; i < Squad.Size; i++)
        {
            int kind = config.Squad[i];
            Squad.Kind[i] = kind;
            Squad.MaxHp[i] = Rules.HeroMaxHp(config.HeroKinds[kind], 0, config.Meta);
            Squad.Hp[i] = Squad.MaxHp[i];
            Squad.Alive[i] = true;
            Squad.AttackCooldown[i] = 20;
        }
        StartAct(0);
    }

    public HeroKind ActiveHero => Config.HeroKinds[Squad.Kind[Squad.Active]];

    public int ModalOption(int index) => _modalOptions[index];

    public bool IsBossAlive => Enemies.IsLive(BossHandle);

    /// <summary>
    /// Один шаг. Порядок систем тика: предыдущие позиции → отряд (ввод, рывок, свап) → места акта → враги (ИИ, телеграфы, выстрелы) →
    /// сетка → расталкивание → сетка → контакты с отрядом → снаряды отряда → снаряды врагов → атака героя → опыт →
    /// директор спавна → конец тика (смерти, освобождение слотов, отложенный спавн, акт, модалки).
    /// </summary>
    public void Step(in InputFrame input)
    {
        if (Outcome != RunOutcome.InProgress)
        {
            return;
        }
        StepCount++;
        if (CollectEvents)
        {
            Events.Clear();
        }
        if (Modal != ModalKind.None)
        {
            ResolveModal(input.Pick);
            return;
        }
        Tick++;
        ActTick++;
        CapturePrevious();
        UpdateSquad(input);
        UpdatePlaces();
        UpdateEnemies();
        Grid.Build(Enemies);
        SeparateEnemies();
        Grid.Build(Enemies);
        ResolveSquadContacts();
        UpdateProjectiles();
        UpdateShots();
        HeroAttack();
        UpdateGems();
        UpdateDirector();
        EndOfTick();
    }

    private void CapturePrevious()
    {
        Squad.PrevX = Squad.X;
        Squad.PrevY = Squad.Y;
        System.Array.Copy(Enemies.X, Enemies.PrevX, Enemies.HighWater);
        System.Array.Copy(Enemies.Y, Enemies.PrevY, Enemies.HighWater);
        System.Array.Copy(Projectiles.X, Projectiles.PrevX, Projectiles.HighWater);
        System.Array.Copy(Projectiles.Y, Projectiles.PrevY, Projectiles.HighWater);
        System.Array.Copy(Shots.X, Shots.PrevX, Shots.HighWater);
        System.Array.Copy(Shots.Y, Shots.PrevY, Shots.HighWater);
        System.Array.Copy(Gems.X, Gems.PrevX, Gems.HighWater);
        System.Array.Copy(Gems.Y, Gems.PrevY, Gems.HighWater);
    }

    private void Emit(SimEventType type, Fix x, Fix y, int kind, int amount = 0, Fix dirX = default, Fix dirY = default,
        bool crit = false, int target = -1, int source = -1)
    {
        if (CollectEvents)
        {
            Events.Add(new SimEvent
            {
                Type = type, X = x, Y = y, Kind = kind, Amount = amount, DirX = dirX, DirY = dirY, Crit = crit, Target = target, Source = source,
            });
        }
    }

    /// <summary>Дайджест всего состояния, влияющего на будущее (презентационные поля и телеметрия — нет).</summary>
    public ulong ComputeDigest()
    {
        var h = new StateHasher();
        h.Add(DigestLayoutVersion);
        h.Add(StepCount);
        h.Add(Tick);
        h.Add(ActIndex);
        h.Add(ActTick);
        h.Add((int)Outcome);
        h.Add((int)Modal);
        h.Add(ModalOptionCount);
        for (int i = 0; i < ModalOptionCount; i++)
        {
            h.Add(_modalOptions[i]);
        }
        h.Add(Level);
        h.Add(Xp);
        h.Add(PendingLevelUps);
        h.Add(BossHandle);
        h.Add(_bossSpawned);
        h.Add(_actCleared);
        h.Add(_nextEliteTick);
        h.Add(_spawnAccumulatorMilli);
        for (int i = 0; i < BlessingLevels.Length; i++)
        {
            h.Add(BlessingLevels[i]);
        }
        Rng.AddTo(ref h);
        Squad.AddTo(ref h);
        Enemies.AddTo(ref h);
        Projectiles.AddTo(ref h);
        Shots.AddTo(ref h);
        Gems.AddTo(ref h);
        Places.AddTo(ref h);
        return h.Finish();
    }
}
