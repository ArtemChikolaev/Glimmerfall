using System.Collections.Generic;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

public sealed partial class World
{
    private const int SpawnCostMilliTicks = 1000 * SimConstants.TicksPerSecond;

    private struct PendingSpawn
    {
        public int Kind;
        public Fix X;
        public Fix Y;
        public bool Boss;
        /// <summary>Место, чей это страж (алтарь риска), или −1.</summary>
        public int Place;
    }

    /// <summary>Отложенный спавн: враги появляются в конце тика (скилл sim-invariants, правило 2).</summary>
    private readonly List<PendingSpawn> _spawnQueue = new();
    private readonly int[] _offerScratch = new int[Blessings.Count];

    private void StartAct(int index)
    {
        ActIndex = index;
        ActTick = 0;
        _bossSpawned = false;
        _actCleared = false;
        BossHandle = -1;
        _nextEliteTick = Config.EliteFirstTicks;
        _spawnAccumulatorMilli = 0;
        for (int i = 0; i < Gems.HighWater; i++)
        {
            if (Gems.Alive[i])
            {
                AddXp(Gems.Value[i]);
            }
        }
        Enemies.Clear();
        Projectiles.Clear();
        Shots.Clear();
        Gems.Clear();
        _spawnQueue.Clear();
        Squad.X = Fix.FromInt(SimConstants.ArenaSize / 2);
        Squad.Y = Fix.FromInt(SimConstants.ArenaSize / 2);
        Squad.PrevX = Squad.X;
        Squad.PrevY = Squad.Y;
        LayoutPlaces();
        ActStats stats = Stats.Acts[index];
        stats.Reached = true;
        stats.StartTick = Tick;
    }

    private void UpdateGems()
    {
        GemPool g = Gems;
        long hx = Squad.X.Raw;
        long hy = Squad.Y.Raw;
        long pickup = Rules.PickupRadius(BlessingLevels[(int)Blessing.Magnet], Config.Meta).Raw;
        long collect = (Rules.GemCollectRadiusPx + ActiveHero.RadiusPx).Raw;
        Fix speed = Rules.GemMagnetSpeedPxPerTick;
        for (int i = 0; i < g.HighWater; i++)
        {
            if (!g.Alive[i])
            {
                continue;
            }
            long dx = hx - g.X[i].Raw;
            long dy = hy - g.Y[i].Raw;
            long d2 = dx * dx + dy * dy;
            if (!g.Magnetized[i] && d2 <= pickup * pickup)
            {
                g.Magnetized[i] = true;
            }
            if (!g.Magnetized[i])
            {
                continue;
            }
            if (d2 <= collect * collect)
            {
                int value = g.Value[i];
                g.Release(i);
                AddXp(value);
                continue;
            }
            int angle = FixMath.Atan2(dy, dx);
            g.X[i] += FixMath.Cos(angle) * speed;
            g.Y[i] += FixMath.Sin(angle) * speed;
        }
    }

    private void AddXp(int value)
    {
        Xp += value;
        while (Xp >= Rules.XpToNext(Level))
        {
            Xp -= Rules.XpToNext(Level);
            Level++;
            PendingLevelUps++;
            Emit(SimEventType.LevelUp, Squad.X, Squad.Y, Level);
        }
    }

    private void UpdateDirector()
    {
        if (Config.Scenario == Scenario.HordeBench)
        {
            QueueBenchRefill();
            return;
        }
        int pending = _spawnQueue.Count;
        _spawnAccumulatorMilli += Rules.SpawnRateMilli(Config, ActIndex, ActTick);
        while (_spawnAccumulatorMilli >= SpawnCostMilliTicks)
        {
            if (Enemies.Count + pending >= Config.MaxEnemies)
            {
                _spawnAccumulatorMilli = SpawnCostMilliTicks;
                break;
            }
            _spawnAccumulatorMilli -= SpawnCostMilliTicks;
            QueueRingSpawn(PickRegularKind(), boss: false);
            pending++;
        }
        int bossTick = Config.ActTicks - Config.BossLeadTicks;
        if (ActTick >= _nextEliteTick && _nextEliteTick < bossTick)
        {
            for (int k = 0; k <= ActIndex; k++)
            {
                QueueRingSpawn(EnemyIds.Ravager, boss: false);
            }
            _nextEliteTick += Config.ElitePeriodTicks;
        }
        if (!_bossSpawned && ActTick >= bossTick)
        {
            QueueRingSpawn(EnemyIds.GloomWarden, boss: true);
            _bossSpawned = true;
        }
    }

    /// <summary>
    /// Вид рядового врага по расписанию акта: в первом акте виды открываются по времени, дальше — все сразу.
    /// Стрелки с 30-й секунды и тараны с минуты заставляют двигаться с первой минуты (столп «угроза»): рядовых
    /// у ног герой ближнего боя рубит и стоя — стоять нельзя из-за тех, до кого он не дотягивается.
    /// </summary>
    private int PickRegularKind()
    {
        int seconds = ActIndex > 0 ? int.MaxValue : ActTick / SimConstants.TicksPerSecond;
        int shambler = 10;
        int runner = seconds >= 45 ? 6 : 0;
        int spitter = seconds >= 30 ? 3 : 0;
        int charger = seconds >= 60 ? 3 : 0;
        int brute = seconds >= 150 ? 2 : 0;
        int roll = Rng[RngStream.Spawn].NextInt(shambler + runner + spitter + charger + brute);
        if ((roll -= shambler) < 0)
        {
            return EnemyIds.Shambler;
        }
        if ((roll -= runner) < 0)
        {
            return EnemyIds.Runner;
        }
        if ((roll -= spitter) < 0)
        {
            return EnemyIds.Spitter;
        }
        if ((roll -= charger) < 0)
        {
            return EnemyIds.Charger;
        }
        return EnemyIds.Brute;
    }

    /// <summary>Кольцо за краем экрана вокруг отряда (400–464 px), с прижатием к арене.</summary>
    private void QueueRingSpawn(int kind, bool boss)
    {
        ref Pcg32 rng = ref Rng[RngStream.Spawn];
        int angle = rng.NextAngle();
        Fix radius = Fix.FromInt(400 + rng.NextInt(64));
        QueueSpawnAt(kind, Squad.X + FixMath.Cos(angle) * radius, Squad.Y + FixMath.Sin(angle) * radius, boss);
    }

    /// <summary>Только для тестов: добить врага в слоте (смерть пройдёт концом следующего тика, как в бою).</summary>
    internal void KillEnemyForTest(int slot) => DamageEnemy(slot, Enemies.Hp[slot], Fix.Zero, Fix.Zero, false, -1);

    /// <summary>Только для тестов: враг в заданной точке сразу, мимо директора.</summary>
    internal int SpawnEnemyForTest(int kind, Fix x, Fix y)
    {
        EnemyKind k = Config.EnemyKinds[kind];
        return Enemies.Spawn(kind, x, y, k.RadiusPx, Rules.EnemyHp(k, Config.Tier, ActIndex), 0);
    }

    private void QueueSpawnAt(int kind, Fix x, Fix y, bool boss, int place = -1)
    {
        Fix margin = Fix.FromInt(24);
        _spawnQueue.Add(new PendingSpawn
        {
            Kind = kind,
            X = Fix.Clamp(x, margin, ArenaMax - margin),
            Y = Fix.Clamp(y, margin, ArenaMax - margin),
            Boss = boss,
            Place = place,
        });
    }

    /// <summary>Спайк орды: доливает врагов до заданного числа в диске 160–1000 px вокруг отряда.</summary>
    private void QueueBenchRefill()
    {
        int deficit = Config.BenchEnemies - Enemies.Count - _spawnQueue.Count;
        int n = deficit < 400 ? deficit : 400;
        ref Pcg32 rng = ref Rng[RngStream.Spawn];
        for (int k = 0; k < n; k++)
        {
            int roll = rng.NextInt(22);
            int kind = roll < 10 ? EnemyIds.Shambler : roll < 16 ? EnemyIds.Runner : roll < 18 ? EnemyIds.Spitter : roll < 20 ? EnemyIds.Charger : EnemyIds.Brute;
            int angle = rng.NextAngle();
            Fix radius = Fix.FromInt(160 + rng.NextInt(840));
            QueueSpawnAt(kind, Squad.X + FixMath.Cos(angle) * radius, Squad.Y + FixMath.Sin(angle) * radius, boss: false);
        }
    }

    private void EndOfTick()
    {
        EnemyPool e = Enemies;
        ActStats act = Stats.Acts[ActIndex];
        foreach (int slot in _deadEnemies)
        {
            EnemyKind kind = Config.EnemyKinds[e.Kind[slot]];
            act.Kills++;
            if (kind.Elite)
            {
                act.EliteKills++;
            }
            if (e.Handle(slot) == BossHandle)
            {
                BossHandle = -1;
                _actCleared = true;
            }
            if (Config.Scenario == Scenario.Run && kind.Xp > 0 && Gems.Spawn(e.X[slot], e.Y[slot], kind.Xp) < 0)
            {
                AddXp(kind.Xp);
            }
            e.Release(slot);
        }
        _deadEnemies.Clear();

        foreach (PendingSpawn spawn in _spawnQueue)
        {
            EnemyKind kind = Config.EnemyKinds[spawn.Kind];
            int cooldown = kind.Behavior switch
            {
                EnemyBehavior.Ranged => Rng[RngStream.Spawn].Range(60, 150),
                EnemyBehavior.Charger => 60,
                EnemyBehavior.Boss => 120,
                _ => 0,
            };
            int slot = e.Spawn(kind.Id, spawn.X, spawn.Y, kind.RadiusPx, Rules.EnemyHp(kind, Config.Tier, ActIndex), cooldown);
            if (spawn.Boss && slot >= 0)
            {
                BossHandle = e.Handle(slot);
            }
            if (spawn.Place >= 0)
            {
                GuardianSpawned(spawn.Place, slot);
            }
        }
        _spawnQueue.Clear();
        if (e.Count > act.PeakEnemies)
        {
            act.PeakEnemies = e.Count;
        }

        if (Outcome != RunOutcome.InProgress)
        {
            return;
        }
        if (_actCleared)
        {
            _actCleared = false;
            act.Cleared = true;
            act.EndTick = Tick;
            act.LevelAtEnd = Level;
            if (ActIndex + 1 >= Config.Acts)
            {
                Outcome = RunOutcome.Victory;
                return;
            }
            OpenCheckpointModal();
            return;
        }
        if (Modal == ModalKind.None && PendingLevelUps > 0)
        {
            OpenBlessingModal();
        }
    }

    private void OpenBlessingModal()
    {
        int candidates = 0;
        for (int b = 0; b < Blessings.Count; b++)
        {
            if (BlessingLevels[b] < Blessings.MaxLevel((Blessing)b))
            {
                _offerScratch[candidates++] = b;
            }
        }
        PendingLevelUps--;
        if (candidates == 0)
        {
            return;
        }
        int count = candidates < MaxModalOptions ? candidates : MaxModalOptions;
        ref Pcg32 rng = ref Rng[RngStream.Offers];
        for (int k = 0; k < count; k++)
        {
            int j = k + rng.NextInt(candidates - k);
            (_offerScratch[k], _offerScratch[j]) = (_offerScratch[j], _offerScratch[k]);
            _modalOptions[k] = _offerScratch[k];
        }
        ModalOptionCount = count;
        Modal = ModalKind.Blessing;
    }

    private void OpenCheckpointModal()
    {
        int count = 0;
        _modalOptions[count++] = (int)CheckpointOption.Heal;
        if (Squad.AliveCount < Squad.Size)
        {
            _modalOptions[count++] = (int)CheckpointOption.Revive;
        }
        _modalOptions[count++] = (int)CheckpointOption.Might;
        ModalOptionCount = count;
        Modal = ModalKind.Checkpoint;
    }

    /// <summary>Разрешает модалку выбором 1..N. Неверный выбор игнорируется — мир стоит; харнесс ловит зацикливание сторожем.</summary>
    private void ResolveModal(byte pick)
    {
        if (pick == 0 || pick > ModalOptionCount)
        {
            return;
        }
        int option = _modalOptions[pick - 1];
        ModalKind modal = Modal;
        Modal = ModalKind.None;
        ModalOptionCount = 0;
        if (modal == ModalKind.Blessing)
        {
            ApplyBlessing((Blessing)option);
        }
        else
        {
            ApplyCheckpoint((CheckpointOption)option);
            StartAct(ActIndex + 1);
        }
        if (PendingLevelUps > 0)
        {
            OpenBlessingModal();
        }
    }

    private void ApplyBlessing(Blessing blessing)
    {
        BlessingLevels[(int)blessing]++;
        Squad s = Squad;
        switch (blessing)
        {
            case Blessing.Vigor:
                for (int i = 0; i < Squad.Size; i++)
                {
                    int max = Rules.HeroMaxHp(Config.HeroKinds[s.Kind[i]], BlessingLevels[(int)Blessing.Vigor], Config.Meta);
                    if (s.Alive[i])
                    {
                        s.Hp[i] += max - s.MaxHp[i];
                    }
                    s.MaxHp[i] = max;
                }
                break;
            case Blessing.Mend:
                Heal(s.Active, Rules.MendHealPercent);
                break;
        }
    }

    private void ApplyCheckpoint(CheckpointOption option)
    {
        Squad s = Squad;
        switch (option)
        {
            case CheckpointOption.Heal:
                for (int i = 0; i < Squad.Size; i++)
                {
                    Heal(i, Rules.CheckpointHealPercent);
                }
                break;
            case CheckpointOption.Revive:
                for (int i = 0; i < Squad.Size; i++)
                {
                    if (!s.Alive[i])
                    {
                        s.Alive[i] = true;
                        s.Hp[i] = Rules.Percent(s.MaxHp[i], Rules.ReviveHpPercent);
                    }
                }
                break;
            case CheckpointOption.Might:
                BlessingLevels[(int)Blessing.Might]++;
                break;
        }
    }

    private void Heal(int hero, int percent)
    {
        Squad s = Squad;
        if (!s.Alive[hero])
        {
            return;
        }
        s.Hp[hero] += Rules.Percent(s.MaxHp[hero], percent);
        if (s.Hp[hero] > s.MaxHp[hero])
        {
            s.Hp[hero] = s.MaxHp[hero];
        }
    }
}
