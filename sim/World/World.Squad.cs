using System.Collections.Generic;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

public sealed partial class World
{
    private static readonly Fix ArenaMax = Fix.FromInt(SimConstants.ArenaSize);
    private const int GoldenAngle = 25033; // ≈137,5° в единицах угла: равномерная спираль без повторов

    /// <summary>Слоты врагов, уже задетых приёмом (цепная молния, ледяная гроза), — рабочий буфер, не состояние.</summary>
    private readonly List<int> _chainHits = new(Rules.IceStormTargets);

    /// <summary>Кандидаты удара дугой и квадраты их расстояний — рабочие буферы, не состояние.</summary>
    private readonly List<int> _cleaveHits = new(32);
    private readonly List<long> _cleaveDistance = new(32);

    /// <summary>Запас поиска цели ближнего боя на радиус крупных врагов (босс — 26 px).</summary>
    private static readonly Fix MeleeTargetSlackPx = Fix.FromInt(28);

    private void UpdateSquad(in InputFrame input)
    {
        Squad s = Squad;
        if (s.DashCooldown > 0)
        {
            s.DashCooldown--;
        }
        if (s.SwapCooldown > 0)
        {
            s.SwapCooldown--;
        }
        if (s.AssistCooldown > 0)
        {
            s.AssistCooldown--;
        }
        if (s.LinkTicks > 0 && --s.LinkTicks == 0)
        {
            s.LinkHelper = -1;
        }
        if (s.InvulnerableTicks > 0)
        {
            s.InvulnerableTicks--;
        }
        if (Tick % Rules.ReserveRegenPeriodTicks == 0)
        {
            RegenerateReserves();
        }
        if ((input.Buttons & InputButtons.Swap) != 0 && s.SwapCooldown == 0)
        {
            int next = NextAliveReserve();
            if (next >= 0)
            {
                SwapTo(next, forced: false);
            }
        }
        if ((input.Buttons & InputButtons.Assist) != 0 && s.AssistCooldown == 0)
        {
            int helper = NextAliveReserve();
            if (helper >= 0)
            {
                Assist(helper);
            }
        }

        HeroKind kind = ActiveHero;
        Fix speed = Rules.MoveSpeed(kind, BlessingLevels[(int)Blessing.Swift], Config.Meta);
        Fix vx = Fix.Zero;
        Fix vy = Fix.Zero;
        long mx = input.MoveX;
        long my = input.MoveY;
        long lengthSquared = mx * mx + my * my;
        if (lengthSquared > 0)
        {
            long length = (long)FixMath.ISqrt((ulong)lengthSquared);
            if (length == 0)
            {
                length = 1;
            }
            long denominator = length > 127 ? length : 127;
            vx = Fix.FromRaw(speed.Raw * mx / denominator);
            vy = Fix.FromRaw(speed.Raw * my / denominator);
            s.FacingX = Fix.FromRaw((mx << Fix.FractionBits) / length);
            s.FacingY = Fix.FromRaw((my << Fix.FractionBits) / length);
        }
        if ((input.Buttons & InputButtons.Dash) != 0 && s.DashCooldown == 0 && s.DashTicks == 0)
        {
            s.DashTicks = Rules.DashTicks;
            s.DashCooldown = Rules.DashCooldownTicks;
            s.DashDirX = s.FacingX;
            s.DashDirY = s.FacingY;
            Stats.Acts[ActIndex].Dashes++;
            Emit(SimEventType.Dash, s.X, s.Y, s.Kind[s.Active]);
        }
        if (s.DashTicks > 0)
        {
            s.DashTicks--;
            Fix dashSpeed = speed * Rules.DashSpeedPercent / 100;
            vx = s.DashDirX * dashSpeed;
            vy = s.DashDirY * dashSpeed;
        }
        Fix r = kind.RadiusPx;
        s.X = Fix.Clamp(s.X + vx, r, ArenaMax - r);
        s.Y = Fix.Clamp(s.Y + vy, r, ArenaMax - r);
    }

    private void RegenerateReserves()
    {
        Squad s = Squad;
        for (int i = 0; i < Squad.Size; i++)
        {
            if (i == s.Active || !s.Alive[i] || s.Hp[i] >= s.MaxHp[i])
            {
                continue;
            }
            int heal = Rules.Percent(s.MaxHp[i], Rules.ReserveRegenPercent);
            s.Hp[i] += heal < 1 ? 1 : heal;
            if (s.Hp[i] > s.MaxHp[i])
            {
                s.Hp[i] = s.MaxHp[i];
            }
        }
    }

    /// <summary>Следующий живой герой резерва по кругу или −1.</summary>
    public int NextAliveReserve()
    {
        for (int k = 1; k < Squad.Size; k++)
        {
            int i = (Squad.Active + k) % Squad.Size;
            if (Squad.Alive[i])
            {
                return i;
            }
        }
        return -1;
    }

    private void SwapTo(int next, bool forced)
    {
        Squad s = Squad;
        HeroKind outgoing = Config.HeroKinds[s.Kind[s.Active]];
        HeroKind kind = Config.HeroKinds[s.Kind[next]];
        // связка (PRD §5): свап по своей воле в окне после ассиста — на того, кто выпрыгнул; любой свап окно закрывает
        bool linked = !forced && s.LinkTicks > 0 && next == s.LinkHelper;
        s.LinkTicks = 0;
        s.LinkHelper = -1;
        s.Active = next;
        s.SwapCooldown = Rules.SwapCooldownTicks;
        s.InvulnerableTicks = forced ? Rules.ForcedSwapInvulnerableTicks : Rules.SwapInvulnerableTicks;
        if (!forced)
        {
            Stats.Acts[ActIndex].Swaps++;
        }
        Emit(SimEventType.Swap, s.X, s.Y, kind.Id, forced ? 1 : 0);
        if (linked && Rules.TryDuet(outgoing.Element, kind.Element, out DuetKind duet))
        {
            Duet(duet, outgoing, kind);
        }
        else
        {
            // вход героя (PRD §5): удар по кругу
            int damage = Rules.SwapStrikeDamage(kind, BlessingLevels[(int)Blessing.Might], Config.Meta);
            Emit(SimEventType.SwapStrike, s.X, s.Y, kind.Id, Rules.KnockbackRadiusPx.FloorToInt());
            StrikeCircle(s.X, s.Y, Rules.KnockbackRadiusPx, damage, kind.Id);
        }
        Knockback(Rules.KnockbackRadiusPx, Rules.KnockbackDistancePx);
    }

    /// <summary>
    /// Ассист (PRD §5): резервный герой выпрыгивает, бьёт фирменным приёмом и уходит; активный остаётся на поле.
    /// Откат общий на отряд; открывает окно связки на этого героя. Перебор врагов — по слотам: порядок детерминирован.
    /// </summary>
    private void Assist(int helper)
    {
        Squad s = Squad;
        HeroKind kind = Config.HeroKinds[s.Kind[helper]];
        int damage = Rules.AssistDamage(kind, BlessingLevels[(int)Blessing.Might], Config.Meta);
        s.AssistCooldown = Rules.AssistCooldownTicks;
        s.LinkTicks = Rules.LinkWindowTicks;
        s.LinkHelper = helper;
        Stats.Acts[ActIndex].Assists++;
        Emit(SimEventType.Assist, s.X, s.Y, kind.Id, Rules.AssistRadius(kind.Assist).FloorToInt(), s.FacingX, s.FacingY);
        switch (kind.Assist)
        {
            case AssistKind.FireRing:
                StrikeCircle(s.X, s.Y, Rules.FireRingRadiusPx, damage, kind.Id);
                break;
            case AssistKind.ShieldBash:
                ShieldBash(damage, kind.Id);
                break;
            case AssistKind.ChainLightning:
                ChainLightning(damage, kind.Id);
                break;
        }
    }

    /// <summary>Урон всем врагам в круге с центром (cx, cy); направление удара — от центра к врагу.</summary>
    private void StrikeCircle(Fix cx, Fix cy, Fix radius, int damage, int source)
    {
        EnemyPool e = Enemies;
        long r2 = radius.Raw * radius.Raw;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            long dx = e.X[i].Raw - cx.Raw;
            long dy = e.Y[i].Raw - cy.Raw;
            if (dx * dx + dy * dy > r2)
            {
                continue;
            }
            DamageEnemy(i, damage, Fix.FromRaw(dx), Fix.FromRaw(dy), false, source);
        }
    }

    /// <summary>Удар щитом: конус ±60° по взгляду отряда, урон и отброс рядовых (элиты и босс стоят).</summary>
    private void ShieldBash(int damage, int source)
    {
        EnemyPool e = Enemies;
        Squad s = Squad;
        long r = Rules.ShieldBashRadiusPx.Raw;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            long dx = e.X[i].Raw - s.X.Raw;
            long dy = e.Y[i].Raw - s.Y.Raw;
            long d2 = dx * dx + dy * dy;
            if (d2 > r * r)
            {
                continue;
            }
            long distance = (long)FixMath.ISqrt((ulong)d2);
            long along = (dx * s.FacingX.Raw + dy * s.FacingY.Raw) >> Fix.FractionBits;
            if (2 * along < distance)
            {
                continue;
            }
            DamageEnemy(i, damage, Fix.FromRaw(dx), Fix.FromRaw(dy), false, source);
            if (!e.Alive[i] || _heavyKind[e.Kind[i]])
            {
                continue;
            }
            int angle = FixMath.Atan2(dy, dx);
            Fix radiusE = e.Radius[i];
            e.X[i] = Fix.Clamp(e.X[i] + FixMath.Cos(angle) * Rules.ShieldBashPushPx, radiusE, ArenaMax - radiusE);
            e.Y[i] = Fix.Clamp(e.Y[i] + FixMath.Sin(angle) * Rules.ShieldBashPushPx, radiusE, ArenaMax - radiusE);
        }
    }

    /// <summary>
    /// Цепная молния: ближайший враг в радиусе первого прыжка, затем до <see cref="Rules.ChainLightningTargets"/> прыжков
    /// к ближайшему ещё не задетому; равные расстояния — меньший слот.
    /// </summary>
    private void ChainLightning(int damage, int source)
    {
        EnemyPool e = Enemies;
        _chainHits.Clear();
        long fromX = Squad.X.Raw;
        long fromY = Squad.Y.Raw;
        for (int hop = 0; hop < Rules.ChainLightningTargets; hop++)
        {
            long reach = (hop == 0 ? Rules.ChainFirstRangePx : Rules.ChainHopPx).Raw;
            long best = reach * reach + 1;
            int target = -1;
            for (int i = 0; i < e.HighWater; i++)
            {
                if (!e.Alive[i] || _chainHits.Contains(i))
                {
                    continue;
                }
                long dx = e.X[i].Raw - fromX;
                long dy = e.Y[i].Raw - fromY;
                long d2 = dx * dx + dy * dy;
                if (d2 < best)
                {
                    best = d2;
                    target = i;
                }
            }
            if (target < 0)
            {
                break;
            }
            long tx = e.X[target].Raw;
            long ty = e.Y[target].Raw;
            Emit(SimEventType.ChainLink, Fix.FromRaw(fromX), Fix.FromRaw(fromY), source, hop, Fix.FromRaw(tx - fromX), Fix.FromRaw(ty - fromY));
            _chainHits.Add(target);
            DamageEnemy(target, damage, Fix.FromRaw(tx - fromX), Fix.FromRaw(ty - fromY), false, source);
            fromX = tx;
            fromY = ty;
        }
    }

    /// <summary>Отбрасывает рядовых врагов из круга вокруг отряда (вход героя). Элиты и босс стоят.</summary>
    private void Knockback(Fix radius, Fix distance)
    {
        EnemyPool e = Enemies;
        long r2 = radius.Raw * radius.Raw;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i] || _heavyKind[e.Kind[i]])
            {
                continue;
            }
            long dx = e.X[i].Raw - Squad.X.Raw;
            long dy = e.Y[i].Raw - Squad.Y.Raw;
            if (dx * dx + dy * dy >= r2)
            {
                continue;
            }
            int angle = FixMath.Atan2(dy, dx);
            Fix radiusE = e.Radius[i];
            e.X[i] = Fix.Clamp(e.X[i] + FixMath.Cos(angle) * distance, radiusE, ArenaMax - radiusE);
            e.Y[i] = Fix.Clamp(e.Y[i] + FixMath.Sin(angle) * distance, radiusE, ArenaMax - radiusE);
        }
    }

    public bool SquadInvulnerable => Squad.InvulnerableTicks > 0 || Squad.DashTicks > 0;

    /// <summary>Урон активному герою. Смерть — вынужденный свап; пал последний — поражение.</summary>
    private void DamageSquad(int amount, int sourceKind)
    {
        if (Config.Scenario == Scenario.HordeBench || SquadInvulnerable || amount <= 0 || Outcome != RunOutcome.InProgress)
        {
            return;
        }
        Squad s = Squad;
        int a = s.Active;
        amount = Rules.DamageTaken(ActiveHero, amount);
        s.Hp[a] -= amount;
        Stats.Acts[ActIndex].DamageTaken += amount;
        Stats.LastDamageKind = sourceKind;
        Emit(SimEventType.HeroHit, s.X, s.Y, sourceKind, amount);
        if (s.Hp[a] > 0)
        {
            return;
        }
        s.Hp[a] = 0;
        s.Alive[a] = false;
        Stats.Acts[ActIndex].HeroDeaths++;
        Emit(SimEventType.HeroDown, s.X, s.Y, s.Kind[a]);
        int next = NextAliveReserve();
        if (next >= 0)
        {
            SwapTo(next, forced: true);
            return;
        }
        Outcome = RunOutcome.Defeat;
        Stats.KillerKind = sourceKind;
        Stats.Acts[ActIndex].WipeActTick = ActTick;
        Stats.Acts[ActIndex].EndTick = Tick;
        Stats.Acts[ActIndex].LevelAtEnd = Level;
    }

    private void HeroAttack()
    {
        Squad s = Squad;
        int a = s.Active;
        if (s.AttackCooldown[a] > 0)
        {
            s.AttackCooldown[a]--;
            return;
        }
        HeroKind kind = ActiveHero;
        if (Config.Scenario == Scenario.HordeBench)
        {
            EmitBenchProjectiles(kind);
            return;
        }
        bool melee = kind.Pattern == AttackPattern.Cleave;
        // ближний бой ищет цель с запасом на радиус крупных врагов: их край в досягаемости раньше центра
        int target = FindTarget(melee ? kind.RangePx + MeleeTargetSlackPx : kind.RangePx);
        if (target < 0)
        {
            return;
        }
        int damage = Rules.HeroDamage(kind, BlessingLevels[(int)Blessing.Might], Config.Meta);
        int count = Rules.ProjectileCount(kind, BlessingLevels[(int)Blessing.Volley]);
        int pierce = Rules.Pierce(kind, BlessingLevels[(int)Blessing.Pierce]);
        int life = (int)(kind.RangePx.Raw / kind.ProjectileSpeedPxPerTick.Raw) + 1;
        int baseAngle = FixMath.Atan2(Enemies.Y[target].Raw - s.Y.Raw, Enemies.X[target].Raw - s.X.Raw);
        switch (kind.Pattern)
        {
            case AttackPattern.Cleave:
                count = Cleave(kind, baseAngle, damage, pierce, Rules.CleaveArc(kind, BlessingLevels[(int)Blessing.Volley]));
                break;
            case AttackPattern.Bolt:
            case AttackPattern.Fan:
                int step = kind.Pattern == AttackPattern.Fan ? kind.SpreadAngle : FixMath.Degrees(6);
                for (int k = 0; k < count; k++)
                {
                    int angle = baseAngle + (2 * k - (count - 1)) * step / 2;
                    FireProjectile(kind, angle, life, damage, pierce);
                }
                break;
            case AttackPattern.Nova:
                for (int k = 0; k < count; k++)
                {
                    FireProjectile(kind, s.SpinAngle + k * FixMath.FullTurn / count, life, damage, pierce);
                }
                s.SpinAngle += FixMath.FullTurn / (2 * count);
                break;
        }
        Emit(SimEventType.HeroAttack, s.X, s.Y, kind.Id, count, FixMath.Cos(baseAngle), FixMath.Sin(baseAngle));
        s.AttackCooldown[a] = Rules.AttackInterval(kind, BlessingLevels[(int)Blessing.Haste]);
    }

    /// <summary>
    /// Удар дугой (ближний бой): враги в секторе ±arc от направления на цель и в досягаемости с их радиусом;
    /// не больше targets — ближние первыми, равные — меньший слот. Крит — тем же потоком, что у снарядов.
    /// Тяжёлый удар (CleavePushPx) отбрасывает рядовых. Возвращает число задетых.
    /// </summary>
    private int Cleave(HeroKind kind, int angle, int damage, int targets, int arc)
    {
        EnemyPool e = Enemies;
        Squad s = Squad;
        long cos = FixMath.Cos(angle).Raw;
        long sin = FixMath.Sin(angle).Raw;
        long cosArc = FixMath.Cos(arc).Raw;
        _cleaveHits.Clear();
        _cleaveDistance.Clear();
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            long dx = e.X[i].Raw - s.X.Raw;
            long dy = e.Y[i].Raw - s.Y.Raw;
            long reach = (kind.RangePx + e.Radius[i]).Raw;
            long d2 = dx * dx + dy * dy;
            if (d2 > reach * reach)
            {
                continue;
            }
            long distance = (long)FixMath.ISqrt((ulong)d2);
            long along = (dx * cos + dy * sin) >> Fix.FractionBits;
            // в секторе — проекция на направление удара не меньше distance·cos(arc); вплотную — всегда
            if (distance > e.Radius[i].Raw && along * Fix.OneRaw < distance * cosArc)
            {
                continue;
            }
            _cleaveHits.Add(i);
            _cleaveDistance.Add(d2);
        }
        int hits = 0;
        while (hits < targets && _cleaveHits.Count > 0)
        {
            int best = 0;
            for (int k = 1; k < _cleaveHits.Count; k++)
            {
                if (_cleaveDistance[k] < _cleaveDistance[best])
                {
                    best = k;
                }
            }
            int slot = _cleaveHits[best];
            _cleaveHits.RemoveAt(best);
            _cleaveDistance.RemoveAt(best);
            long dx = e.X[slot].Raw - s.X.Raw;
            long dy = e.Y[slot].Raw - s.Y.Raw;
            bool crit = Rng[RngStream.Combat].Chance(Fix.Ratio(Rules.CritPercent, 100));
            DamageEnemy(slot, crit ? damage * 2 : damage, Fix.FromRaw(dx), Fix.FromRaw(dy), crit, kind.Id);
            hits++;
            if (kind.CleavePushPx.Raw > 0 && e.Alive[slot] && !_heavyKind[e.Kind[slot]])
            {
                int away = FixMath.Atan2(dy, dx);
                Fix radius = e.Radius[slot];
                e.X[slot] = Fix.Clamp(e.X[slot] + FixMath.Cos(away) * kind.CleavePushPx, radius, ArenaMax - radius);
                e.Y[slot] = Fix.Clamp(e.Y[slot] + FixMath.Sin(away) * kind.CleavePushPx, radius, ArenaMax - radius);
            }
        }
        return hits;
    }

    private void FireProjectile(HeroKind kind, int angle, int life, int damage, int pierce)
    {
        Fix cos = FixMath.Cos(angle);
        Fix sin = FixMath.Sin(angle);
        Fix speed = kind.ProjectileSpeedPxPerTick;
        Projectiles.Spawn(Squad.X, Squad.Y, cos * speed, sin * speed, kind.ProjectileRadiusPx, life, damage, pierce, kind.Id);
    }

    /// <summary>Спайк орды: держит заданное число снарядов спиралью от героя (вместо прицельной атаки).</summary>
    private void EmitBenchProjectiles(HeroKind kind)
    {
        int deficit = Config.BenchProjectiles - Projectiles.Count;
        int n = deficit < 48 ? deficit : 48;
        Fix speed = Fix.PerSecond(180);
        for (int k = 0; k < n; k++)
        {
            int angle = Squad.SpinAngle;
            Squad.SpinAngle += GoldenAngle;
            Projectiles.Spawn(Squad.X, Squad.Y, FixMath.Cos(angle) * speed, FixMath.Sin(angle) * speed,
                kind.ProjectileRadiusPx, 180, kind.Damage, 3, kind.Id);
        }
    }

    /// <summary>Цель автоатаки: ближайшая элита или босс в радиусе, иначе ближайший враг; −1 — никого.</summary>
    public int FindTarget(Fix range)
    {
        long best = range.Raw * range.Raw;
        long bestHeavy = best;
        int bestSlot = -1;
        int bestHeavySlot = -1;
        int span = (range.FloorToInt() >> SpatialGrid.CellShift) + 1;
        int cx = SpatialGrid.CellCoord(Squad.X);
        int cy = SpatialGrid.CellCoord(Squad.Y);
        int x0 = cx - span < 0 ? 0 : cx - span;
        int x1 = cx + span >= SpatialGrid.Cells ? SpatialGrid.Cells - 1 : cx + span;
        int y0 = cy - span < 0 ? 0 : cy - span;
        int y1 = cy + span >= SpatialGrid.Cells ? SpatialGrid.Cells - 1 : cy + span;
        EnemyPool e = Enemies;
        for (int gy = y0; gy <= y1; gy++)
        {
            for (int gx = x0; gx <= x1; gx++)
            {
                int cell = gy * SpatialGrid.Cells + gx;
                for (int k = Grid.CellStart[cell]; k < Grid.CellStart[cell + 1]; k++)
                {
                    int i = Grid.Items[k];
                    if (!e.Alive[i])
                    {
                        continue;
                    }
                    long dx = e.X[i].Raw - Squad.X.Raw;
                    long dy = e.Y[i].Raw - Squad.Y.Raw;
                    long d2 = dx * dx + dy * dy;
                    if (d2 < best)
                    {
                        best = d2;
                        bestSlot = i;
                    }
                    if (d2 < bestHeavy && _heavyKind[e.Kind[i]])
                    {
                        bestHeavy = d2;
                        bestHeavySlot = i;
                    }
                }
            }
        }
        return bestHeavySlot >= 0 ? bestHeavySlot : bestSlot;
    }
}
