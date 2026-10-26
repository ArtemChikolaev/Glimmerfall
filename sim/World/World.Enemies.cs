using System.Collections.Generic;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

public sealed partial class World
{
    /// <summary>Таблица расстояний для расталкивания: SqrtPx[i] = √i в Q16.16, i — квадрат расстояния в целых пикселях².
    /// Считается целочисленно при загрузке — без float и без корня на каждую пару.</summary>
    private static readonly long[] SqrtPx = BuildSqrtTable();
    private static readonly long[] InvSqrtPx = BuildInvSqrtTable();
    private const int SqrtTableSize = 4096;

    private readonly long[] _separationX;
    private readonly long[] _separationY;
    private readonly List<int> _heavy = new();
    private readonly List<int> _deadEnemies = new();

    private static long[] BuildSqrtTable()
    {
        var table = new long[SqrtTableSize];
        for (int i = 0; i < SqrtTableSize; i++)
        {
            table[i] = (long)FixMath.ISqrt((ulong)i << (2 * Fix.FractionBits));
        }
        return table;
    }

    private static long[] BuildInvSqrtTable()
    {
        var table = new long[SqrtTableSize];
        for (int i = 1; i < SqrtTableSize; i++)
        {
            table[i] = (1L << (2 * Fix.FractionBits)) / SqrtPx[i];
        }
        return table;
    }

    private void UpdateEnemies()
    {
        EnemyPool e = Enemies;
        long hx = Squad.X.Raw;
        long hy = Squad.Y.Raw;
        Fix heroRadius = ActiveHero.RadiusPx;
        bool enraged = ActTick > Config.ActTicks + Config.BossEnrageTicks;
        _heavy.Clear();
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            if (e.ContactCooldown[i] > 0)
            {
                e.ContactCooldown[i]--;
            }
            if (e.HitFlash[i] > 0)
            {
                e.HitFlash[i]--;
            }
            if (e.Cooldown[i] > 0)
            {
                e.Cooldown[i]--;
            }
            EnemyKind kind = Config.EnemyKinds[e.Kind[i]];
            if (kind.Elite)
            {
                _heavy.Add(i);
            }
            long dx = hx - e.X[i].Raw;
            long dy = hy - e.Y[i].Raw;
            long d2 = dx * dx + dy * dy;
            int angle = FixMath.Atan2(dy, dx);
            Fix dirX = FixMath.Cos(angle);
            Fix dirY = FixMath.Sin(angle);
            Fix speed = kind.SpeedPxPerTick;
            if (enraged && kind.Behavior == EnemyBehavior.Boss)
            {
                speed *= 2;
            }
            long touch = (e.Radius[i] + heroRadius + Rules.ChaseStopPx).Raw;
            bool touching = d2 <= touch * touch;
            Fix vx = Fix.Zero;
            Fix vy = Fix.Zero;
            switch (kind.Behavior)
            {
                case EnemyBehavior.Chase:
                    if (!touching)
                    {
                        vx = dirX * speed;
                        vy = dirY * speed;
                    }
                    break;
                case EnemyBehavior.Ranged:
                    StepRanged(i, kind, d2, dirX, dirY, speed, ref vx, ref vy);
                    break;
                case EnemyBehavior.Charger:
                    StepCharger(i, kind, d2, dirX, dirY, speed, touching, ref vx, ref vy);
                    break;
                case EnemyBehavior.Boss:
                    StepBoss(i, kind, d2, dirX, dirY, speed, touching, enraged, ref vx, ref vy);
                    break;
            }
            Fix r = e.Radius[i];
            e.X[i] = Fix.Clamp(e.X[i] + vx, r, ArenaMax - r);
            e.Y[i] = Fix.Clamp(e.Y[i] + vy, r, ArenaMax - r);
        }
    }

    private static long Squared(Fix value) => value.Raw * value.Raw;

    private void StepRanged(int i, EnemyKind kind, long d2, Fix dirX, Fix dirY, Fix speed, ref Fix vx, ref Fix vy)
    {
        EnemyPool e = Enemies;
        if (e.State[i] == EnemyState.Telegraph)
        {
            if (--e.StateTicks[i] > 0)
            {
                return;
            }
            Fix offset = e.Radius[i] + Rules.ShotRadiusPx;
            Shots.Spawn(e.X[i] + e.DirX[i] * offset, e.Y[i] + e.DirY[i] * offset,
                e.DirX[i] * kind.ShotSpeedPxPerTick, e.DirY[i] * kind.ShotSpeedPxPerTick,
                Rules.ShotRadiusPx, 4 * SimConstants.TicksPerSecond,
                Rules.EnemyDamage(kind.ShotDamage, Config.Tier, ActIndex), kind.Id);
            Emit(SimEventType.EnemyFired, e.X[i], e.Y[i], kind.Id);
            e.State[i] = EnemyState.Normal;
            e.Cooldown[i] = kind.FireCooldownTicks;
            return;
        }
        Fix range = kind.PreferredRangePx;
        if (d2 > Squared(range * 115 / 100))
        {
            vx = dirX * speed;
            vy = dirY * speed;
        }
        else if (d2 < Squared(range * 70 / 100))
        {
            vx = -dirX * speed;
            vy = -dirY * speed;
        }
        if (e.Cooldown[i] == 0 && d2 <= Squared(range * 130 / 100))
        {
            e.State[i] = EnemyState.Telegraph;
            e.StateTicks[i] = kind.AimTicks;
            e.DirX[i] = dirX;
            e.DirY[i] = dirY;
            vx = Fix.Zero;
            vy = Fix.Zero;
        }
    }

    private void StepCharger(int i, EnemyKind kind, long d2, Fix dirX, Fix dirY, Fix speed, bool touching, ref Fix vx, ref Fix vy)
    {
        EnemyPool e = Enemies;
        switch (e.State[i])
        {
            case EnemyState.Telegraph:
                if (--e.StateTicks[i] <= 0)
                {
                    e.State[i] = EnemyState.Charging;
                    e.StateTicks[i] = kind.ChargeTicks;
                }
                return;
            case EnemyState.Charging:
                vx = e.DirX[i] * kind.ChargeSpeedPxPerTick;
                vy = e.DirY[i] * kind.ChargeSpeedPxPerTick;
                if (--e.StateTicks[i] <= 0)
                {
                    e.State[i] = EnemyState.Normal;
                    e.Cooldown[i] = kind.ChargeCooldownTicks;
                }
                return;
        }
        if (e.Cooldown[i] == 0 && d2 <= Squared(kind.ChargeTriggerPx))
        {
            e.State[i] = EnemyState.Telegraph;
            e.StateTicks[i] = kind.ChargeWindupTicks;
            e.DirX[i] = dirX;
            e.DirY[i] = dirY;
            return;
        }
        if (!touching)
        {
            vx = dirX * speed;
            vy = dirY * speed;
        }
    }

    private void StepBoss(int i, EnemyKind kind, long d2, Fix dirX, Fix dirY, Fix speed, bool touching, bool enraged, ref Fix vx, ref Fix vy)
    {
        EnemyPool e = Enemies;
        if (e.State[i] == EnemyState.Telegraph)
        {
            if (--e.StateTicks[i] > 0)
            {
                return;
            }
            Fix reach = kind.SlamRadiusPx + ActiveHero.RadiusPx;
            long hx = Squad.X.Raw - e.X[i].Raw;
            long hy = Squad.Y.Raw - e.Y[i].Raw;
            if (hx * hx + hy * hy <= Squared(reach))
            {
                int damage = Rules.EnemyDamage(kind.SlamDamage, Config.Tier, ActIndex);
                DamageSquad(enraged ? damage * 2 : damage, kind.Id);
            }
            Emit(SimEventType.Slam, e.X[i], e.Y[i], kind.Id, kind.SlamRadiusPx.FloorToInt());
            e.State[i] = EnemyState.Normal;
            e.Cooldown[i] = kind.SlamCooldownTicks;
            return;
        }
        if (e.Cooldown[i] == 0 && d2 <= Squared(kind.SlamRadiusPx * 3 / 4))
        {
            e.State[i] = EnemyState.Telegraph;
            e.StateTicks[i] = kind.SlamWindupTicks;
            return;
        }
        if (!touching)
        {
            vx = dirX * speed;
            vy = dirY * speed;
        }
    }

    /// <summary>
    /// Расталкивание якоби: толчки считаются от позиций начала прохода и применяются разом — результат не зависит
    /// от порядка пар. Рядовые пары — по половине соседства сетки (каждая пара один раз), тяжёлые (элиты, босс) — отдельным
    /// проходом с радиусом в две ячейки: их радиус больше ячейки.
    /// </summary>
    private void SeparateEnemies()
    {
        EnemyPool e = Enemies;
        int high = e.HighWater;
        System.Array.Clear(_separationX, 0, high);
        System.Array.Clear(_separationY, 0, high);
        const int n = SpatialGrid.Cells;
        int[] start = Grid.CellStart;
        int[] items = Grid.Items;
        for (int cy = 0; cy < n; cy++)
        {
            for (int cx = 0; cx < n; cx++)
            {
                int c = cy * n + cx;
                int end = start[c + 1];
                for (int a = start[c]; a < end; a++)
                {
                    int i = items[a];
                    if (_heavyKind[e.Kind[i]])
                    {
                        continue;
                    }
                    for (int b = a + 1; b < end; b++)
                    {
                        SeparatePair(i, items[b]);
                    }
                    if (cx + 1 < n)
                    {
                        SeparateWithCell(i, c + 1);
                    }
                    if (cy + 1 < n)
                    {
                        if (cx > 0)
                        {
                            SeparateWithCell(i, c + n - 1);
                        }
                        SeparateWithCell(i, c + n);
                        if (cx + 1 < n)
                        {
                            SeparateWithCell(i, c + n + 1);
                        }
                    }
                }
            }
        }
        foreach (int i in _heavy)
        {
            SeparateHeavy(i);
        }
        long cap = Rules.MaxSeparationPushPx.Raw;
        for (int i = 0; i < high; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            long px = _separationX[i];
            long py = _separationY[i];
            if (px == 0 && py == 0)
            {
                continue;
            }
            px = px > cap ? cap : px < -cap ? -cap : px;
            py = py > cap ? cap : py < -cap ? -cap : py;
            Fix r = e.Radius[i];
            e.X[i] = Fix.Clamp(Fix.FromRaw(e.X[i].Raw + px), r, ArenaMax - r);
            e.Y[i] = Fix.Clamp(Fix.FromRaw(e.Y[i].Raw + py), r, ArenaMax - r);
        }
    }

    private void SeparateWithCell(int i, int cell)
    {
        int end = Grid.CellStart[cell + 1];
        for (int k = Grid.CellStart[cell]; k < end; k++)
        {
            SeparatePair(i, Grid.Items[k]);
        }
    }

    /// <summary>Толкает пару рядовых врагов друг от друга по четверти перекрытия каждому.</summary>
    private void SeparatePair(int i, int j)
    {
        EnemyPool e = Enemies;
        if (_heavyKind[e.Kind[j]])
        {
            return;
        }
        long dx = e.X[j].Raw - e.X[i].Raw;
        long dy = e.Y[j].Raw - e.Y[i].Raw;
        long rs = e.Radius[i].Raw + e.Radius[j].Raw;
        if (dx >= rs || -dx >= rs || dy >= rs || -dy >= rs)
        {
            return;
        }
        long d2 = dx * dx + dy * dy;
        if (d2 >= rs * rs)
        {
            return;
        }
        PushApart(i, j, dx, dy, d2, rs, rs >> 2, rs >> 2);
    }

    /// <summary>Тяжёлый враг расталкивает рядовых полным перекрытием и не сдвигается сам; тяжёлые между собой — поровну.</summary>
    private void SeparateHeavy(int i)
    {
        EnemyPool e = Enemies;
        int cx = SpatialGrid.CellCoord(e.X[i]);
        int cy = SpatialGrid.CellCoord(e.Y[i]);
        for (int gy = cy - 2; gy <= cy + 2; gy++)
        {
            if (gy < 0 || gy >= SpatialGrid.Cells)
            {
                continue;
            }
            for (int gx = cx - 2; gx <= cx + 2; gx++)
            {
                if (gx < 0 || gx >= SpatialGrid.Cells)
                {
                    continue;
                }
                int cell = gy * SpatialGrid.Cells + gx;
                for (int k = Grid.CellStart[cell]; k < Grid.CellStart[cell + 1]; k++)
                {
                    int j = Grid.Items[k];
                    bool heavyJ = _heavyKind[e.Kind[j]];
                    if (j == i || (heavyJ && j < i))
                    {
                        continue;
                    }
                    long dx = e.X[j].Raw - e.X[i].Raw;
                    long dy = e.Y[j].Raw - e.Y[i].Raw;
                    long rs = e.Radius[i].Raw + e.Radius[j].Raw;
                    long d2 = dx * dx + dy * dy;
                    if (d2 >= rs * rs)
                    {
                        continue;
                    }
                    if (heavyJ)
                    {
                        PushApart(i, j, dx, dy, d2, rs, rs >> 2, rs >> 2);
                    }
                    else
                    {
                        PushApart(i, j, dx, dy, d2, rs, 0, rs);
                    }
                }
            }
        }
    }

    /// <summary>Доли перекрытия задаются как доли суммы радиусов: shareI/rs и shareJ/rs от перекрытия.</summary>
    private void PushApart(int i, int j, long dx, long dy, long d2, long rs, long shareI, long shareJ)
    {
        int d2px = (int)(d2 >> (2 * Fix.FractionBits));
        long ux;
        long uy;
        long d;
        if (d2px == 0)
        {
            ux = Fix.OneRaw;
            uy = 0;
            d = 0;
        }
        else
        {
            if (d2px >= SqrtTableSize)
            {
                return;
            }
            d = SqrtPx[d2px];
            long inv = InvSqrtPx[d2px];
            ux = (dx * inv) >> Fix.FractionBits;
            uy = (dy * inv) >> Fix.FractionBits;
        }
        long overlap = rs - d;
        if (overlap <= 0)
        {
            return;
        }
        long pushI = overlap * shareI / rs;
        long pushJ = overlap * shareJ / rs;
        _separationX[i] -= (ux * pushI) >> Fix.FractionBits;
        _separationY[i] -= (uy * pushI) >> Fix.FractionBits;
        _separationX[j] += (ux * pushJ) >> Fix.FractionBits;
        _separationY[j] += (uy * pushJ) >> Fix.FractionBits;
    }

    /// <summary>Контакты с отрядом: выталкивание врагов из героя и урон касанием с откатом на врага.</summary>
    private void ResolveSquadContacts()
    {
        EnemyPool e = Enemies;
        Fix heroRadius = ActiveHero.RadiusPx;
        int cx = SpatialGrid.CellCoord(Squad.X);
        int cy = SpatialGrid.CellCoord(Squad.Y);
        for (int gy = cy - 2; gy <= cy + 2; gy++)
        {
            if (gy < 0 || gy >= SpatialGrid.Cells)
            {
                continue;
            }
            for (int gx = cx - 2; gx <= cx + 2; gx++)
            {
                if (gx < 0 || gx >= SpatialGrid.Cells)
                {
                    continue;
                }
                int cell = gy * SpatialGrid.Cells + gx;
                for (int k = Grid.CellStart[cell]; k < Grid.CellStart[cell + 1]; k++)
                {
                    int j = Grid.Items[k];
                    if (!e.Alive[j])
                    {
                        continue;
                    }
                    long dx = e.X[j].Raw - Squad.X.Raw;
                    long dy = e.Y[j].Raw - Squad.Y.Raw;
                    long rs = (e.Radius[j] + heroRadius).Raw;
                    long reach = rs + Rules.ContactReachPx.Raw;
                    long d2 = dx * dx + dy * dy;
                    if (d2 >= reach * reach)
                    {
                        continue;
                    }
                    EnemyKind kind = Config.EnemyKinds[e.Kind[j]];
                    if (e.ContactCooldown[j] == 0)
                    {
                        e.ContactCooldown[j] = Rules.ContactCooldownTicks;
                        int damage = Rules.EnemyDamage(kind.ContactDamage, Config.Tier, ActIndex);
                        bool enraged = kind.Behavior == EnemyBehavior.Boss && ActTick > Config.ActTicks + Config.BossEnrageTicks;
                        DamageSquad(enraged ? damage * 2 : damage, kind.Id);
                        if (Outcome != RunOutcome.InProgress)
                        {
                            return;
                        }
                    }
                    if (_heavyKind[kind.Id] || d2 >= rs * rs)
                    {
                        continue;
                    }
                    int d2px = (int)(d2 >> (2 * Fix.FractionBits));
                    if (d2px == 0 || d2px >= SqrtTableSize)
                    {
                        continue;
                    }
                    long overlap = rs - SqrtPx[d2px];
                    if (overlap <= 0)
                    {
                        continue;
                    }
                    long inv = InvSqrtPx[d2px];
                    Fix r = e.Radius[j];
                    e.X[j] = Fix.Clamp(Fix.FromRaw(e.X[j].Raw + ((((dx * inv) >> Fix.FractionBits) * overlap) >> Fix.FractionBits)), r, ArenaMax - r);
                    e.Y[j] = Fix.Clamp(Fix.FromRaw(e.Y[j].Raw + ((((dy * inv) >> Fix.FractionBits) * overlap) >> Fix.FractionBits)), r, ArenaMax - r);
                }
            }
        }
    }

    /// <summary>Урон врагу; смерть снимает Alive сразу (по нему больше не попадут), а слот освобождается в конце тика.</summary>
    /// <summary>Урон врагу; направление, крит и источник идут только в событие для презентации.</summary>
    private void DamageEnemy(int slot, int damage, Fix dirX, Fix dirY, bool crit, int sourceHero)
    {
        EnemyPool e = Enemies;
        if (sourceHero >= 0 && Squad.ShrineTicks > 0)
        {
            damage = Rules.ShrineDamage(damage);
        }
        e.Hp[slot] -= damage;
        e.HitFlash[slot] = 6;
        Stats.Acts[ActIndex].DamageDealt += damage;
        int handle = e.Handle(slot);
        if (e.Hp[slot] > 0)
        {
            Emit(SimEventType.EnemyHit, e.X[slot], e.Y[slot], e.Kind[slot], damage, dirX, dirY, crit, handle, sourceHero);
            return;
        }
        e.Alive[slot] = false;
        _deadEnemies.Add(slot);
        Emit(SimEventType.EnemyKilled, e.X[slot], e.Y[slot], e.Kind[slot], damage, dirX, dirY, crit, handle, sourceHero);
    }
}
