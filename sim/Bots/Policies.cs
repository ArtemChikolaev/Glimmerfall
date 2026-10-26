using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Input;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Bots;

/// <summary>
/// Политика бота: читает состояние мира и возвращает ввод на шаг. Только целочисленная математика — бот лежит в Sim.dll
/// и подчиняется тем же запретам, поэтому бот-прогон даёт одинаковые дайджесты на всех ОС.
/// Модалку бот разрешает раньше любых мировых действий (скилл sim-invariants, правило 1).
/// </summary>
public interface IBotPolicy
{
    string Name { get; }

    InputFrame Decide(World world);
}

/// <summary>Стоит на месте; в модалке берёт первый вариант. Гейт столпа «угроза»: обязан умирать.</summary>
public sealed class AfkPolicy : IBotPolicy
{
    public string Name => "afk";

    public InputFrame Decide(World world) =>
        world.Modal != ModalKind.None ? InputFrame.Choose(0) : default;
}

/// <summary>Играет политикой <paramref name="before"/> до заданной секунды мира, потом стоит (замер «AFK ко 2-й минуте»).</summary>
public sealed class AfkAfterPolicy : IBotPolicy
{
    private readonly IBotPolicy _before;
    private readonly int _afterTicks;
    private readonly AfkPolicy _afk = new();

    public AfkAfterPolicy(IBotPolicy before, int afterSeconds)
    {
        _before = before;
        _afterTicks = afterSeconds * SimConstants.TicksPerSecond;
        AfterSeconds = afterSeconds;
    }

    public int AfterSeconds { get; }

    public string Name => $"afk-after-{AfterSeconds}s";

    public InputFrame Decide(World world) => world.Tick < _afterTicks ? _before.Decide(world) : _afk.Decide(world);
}

/// <summary>
/// Кайтер (идея sim_arcade из aegis-draft, переписанная под отряд): уходит от взвешенного центра масс ближних врагов
/// с поворотом по кругу, уклоняется от телеграфов (удар босса, линия тарана, снаряды), собирает опыт, держится от стен,
/// рывком выходит из окружения, свапает героя на низком здоровье. Выборы: благословения по приоритету, на чекпоинте —
/// воскрешение, затем лечение.
/// </summary>
public sealed class KiterPolicy : IBotPolicy
{
    private static readonly long DangerRadius = Fix.FromInt(200).Raw;
    private static readonly long CloseRadius = Fix.FromInt(70).Raw;
    private static readonly long GemSearchRadius = Fix.FromInt(320).Raw;
    private static readonly long WallMargin = Fix.FromInt(260).Raw;
    private static readonly int OrbitAngle = FixMath.Degrees(35);
    private const int HeadingStep = FixMath.FullTurn / 64;
    private const int LinkReactionTicks = 30;
    /// <summary>Вес толпы вплотную (рядовой 1, громила 2, элита 4), выше которого ближний бой отходит, а не рубит.</summary>
    private const int MeleeCrowdLimit = 5;
    /// <summary>За сколько тиков до готовности удара ближний бой шагает к врагу.</summary>
    private const int MeleeStepInTicks = 10;
    /// <summary>Святилище — только при толпе вплотную легче этого веса и в пределах этой дальности.</summary>
    private const int ShrineCrowdLimit = 4;
    private static readonly long ShrineSearchRadius = Fix.FromInt(900).Raw;

    public string Name => "kiter";

    public InputFrame Decide(World world)
    {
        if (world.Modal == ModalKind.Blessing)
        {
            return InputFrame.Choose(PickBlessing(world));
        }
        if (world.Modal == ModalKind.Checkpoint)
        {
            return InputFrame.Choose(PickCheckpoint(world));
        }

        Squad s = world.Squad;
        long hx = s.X.Raw;
        long hy = s.Y.Raw;
        int active = s.Active;
        int hpPercent = s.Hp[active] * 100 / s.MaxHp[active];
        long fx = 0;
        long fy = 0;
        bool urgent = false;
        bool late = false;

        EnemyPool e = world.Enemies;
        SimConfig config = world.Config;
        int boss = world.IsBossAlive ? world.BossHandle & 0xFFFF : -1;
        bool engage = boss >= 0 && hpPercent > 35 && e.State[boss] != EnemyState.Telegraph;
        long crowdX = 0;
        long crowdY = 0;
        int danger = 0;
        // ближний бой (удар дугой) воюет у края толпы: подходит на досягаемость, пока вокруг не густо
        bool melee = world.ActiveHero.Pattern == AttackPattern.Cleave;
        long nearest = long.MaxValue;
        long nearestX = 0;
        long nearestY = 0;
        EnemyKind? bossKind = null;
        long bossX = 0;
        long bossY = 0;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            long dx = e.X[i].Raw - hx;
            long dy = e.Y[i].Raw - hy;
            EnemyKind kind = config.EnemyKinds[e.Kind[i]];
            if (kind.Behavior == EnemyBehavior.Boss && e.State[i] == EnemyState.Telegraph)
            {
                long reach = (kind.SlamRadiusPx + Fix.FromInt(30)).Raw;
                if (dx * dx + dy * dy < reach * reach)
                {
                    AddAway(ref fx, ref fy, dx, dy, 8);
                    urgent = true;
                    late |= e.StateTicks[i] < 20;
                }
            }
            if (i == boss && engage)
            {
                bossKind = kind;
                bossX = dx;
                bossY = dy;
                continue;
            }
            if (kind.Behavior == EnemyBehavior.Charger && e.State[i] != EnemyState.Normal)
            {
                if (DodgeLine(e.X[i].Raw, e.Y[i].Raw, e.DirX[i].Raw, e.DirY[i].Raw, Fix.FromInt(170).Raw,
                        (e.Radius[i] + Fix.FromInt(20)).Raw, hx, hy, ref fx, ref fy))
                {
                    urgent = true;
                    late |= e.State[i] == EnemyState.Charging;
                }
            }
            if (dx > DangerRadius || -dx > DangerRadius || dy > DangerRadius || -dy > DangerRadius)
            {
                continue;
            }
            long d2 = dx * dx + dy * dy;
            if (d2 > DangerRadius * DangerRadius)
            {
                continue;
            }
            long d = (long)FixMath.ISqrt((ulong)d2);
            if (d < nearest)
            {
                nearest = d;
                nearestX = dx;
                nearestY = dy;
            }
            int weight = kind.Elite ? 4 : kind.Id == EnemyIds.Brute ? 2 : 1;
            long proximity = (DangerRadius - d) * weight;
            int angle = FixMath.Atan2(dy, dx);
            crowdX += (FixMath.Cos(angle).Raw * proximity) >> Fix.FractionBits;
            crowdY += (FixMath.Sin(angle).Raw * proximity) >> Fix.FractionBits;
            if (d < CloseRadius)
            {
                danger += weight;
            }
        }

        ShotPool shots = world.Shots;
        for (int i = 0; i < shots.HighWater; i++)
        {
            if (!shots.Alive[i])
            {
                continue;
            }
            if (DodgeLine(shots.X[i].Raw, shots.Y[i].Raw, shots.VX[i].Raw, shots.VY[i].Raw, Fix.FromInt(110).Raw,
                    Fix.FromInt(16).Raw, hx, hy, ref fx, ref fy, directionIsVelocity: true))
            {
                urgent = true;
            }
        }

        // удар-и-отход: к врагу — только когда удар готов, между ударами — обычное кайтование
        // к боссу ближний бой идёт, только пока вокруг не слишком густо: иначе орда смыкается вокруг, пока он рубит
        // босса; порог вдвое выше, чем для рядовых, — иначе во втором акте, где толпа есть всегда, босс не умирает
        if (bossKind != null && (!melee || danger < 2 * MeleeCrowdLimit))
        {
            EngageBoss(world, bossKind, bossX, bossY, ref fx, ref fy);
        }
        bool brawl = melee && s.AttackCooldown[active] <= MeleeStepInTicks && danger < MeleeCrowdLimit && hpPercent > 40 && !urgent;
        if (crowdX != 0 || crowdY != 0)
        {
            int away = FixMath.Atan2(-crowdY, -crowdX) + OrbitAngle;
            int strength = brawl ? 1 : danger > 0 ? 3 : 2;
            fx += FixMath.Cos(away).Raw * strength;
            fy += FixMath.Sin(away).Raw * strength;
        }
        if (brawl && nearest != long.MaxValue && nearest > world.ActiveHero.RangePx.Raw * 8 / 10)
        {
            int toward = FixMath.Atan2(nearestY, nearestX);
            fx += FixMath.Cos(toward).Raw * 3;
            fy += FixMath.Sin(toward).Raw * 3;
        }

        int gem = NearestGem(world, hx, hy);
        if (gem >= 0 && !urgent)
        {
            long gx = world.Gems.X[gem].Raw - hx;
            long gy = world.Gems.Y[gem].Raw - hy;
            int toward = FixMath.Atan2(gy, gx);
            int pull = danger > 2 ? 1 : 2;
            fx += FixMath.Cos(toward).Raw * pull;
            fy += FixMath.Sin(toward).Raw * pull;
        }

        // святилище: пока вокруг не густо и босса нет — к ближнему нетронутому святилищу и стоять в его круге
        int shrine = NearestDormantShrine(world, hx, hy);
        if (shrine >= 0 && danger < ShrineCrowdLimit && boss < 0 && !urgent)
        {
            long sx = world.Places.X[shrine].Raw - hx;
            long sy = world.Places.Y[shrine].Raw - hy;
            if (sx * sx + sy * sy <= Rules.PlaceRadiusPx.Raw * Rules.PlaceRadiusPx.Raw / 3)
            {
                fx = 0;
                fy = 0;
            }
            else
            {
                int toward = FixMath.Atan2(sy, sx);
                fx += FixMath.Cos(toward).Raw * 4;
                fy += FixMath.Sin(toward).Raw * 4;
            }
        }

        long max = Fix.FromInt(SimConstants.ArenaSize).Raw;
        if (hx < WallMargin) fx += (WallMargin - hx) * 3 * Fix.OneRaw / WallMargin;
        if (hx > max - WallMargin) fx -= (hx - (max - WallMargin)) * 3 * Fix.OneRaw / WallMargin;
        if (hy < WallMargin) fy += (WallMargin - hy) * 3 * Fix.OneRaw / WallMargin;
        if (hy > max - WallMargin) fy -= (hy - (max - WallMargin)) * 3 * Fix.OneRaw / WallMargin;

        InputButtons buttons = InputButtons.None;
        int reserve = world.NextAliveReserve();
        if (reserve >= 0 && s.SwapCooldown == 0 && hpPercent < 30 && s.Hp[reserve] * 100 / s.MaxHp[reserve] > 55)
        {
            buttons |= InputButtons.Swap;
        }
        if (s.DashCooldown == 0 && ((urgent && late) || (danger >= 6 && hpPercent < 50)))
        {
            buttons |= InputButtons.Dash;
        }
        // ассист — когда рядом толпа: приём резервного героя бьёт по площади
        if (reserve >= 0 && s.AssistCooldown == 0 && danger >= 6)
        {
            buttons |= InputButtons.Assist;
        }
        // связка: окно после ассиста открыто — свап на выпрыгнувшего героя, если он не на исходе сил; с реакцией
        // человека, а не в следующий тик — и приём помощника успевает доиграть на экране (демо на титуле — этот же бот)
        if (s.LinkTicks > 0 && s.LinkTicks <= Rules.LinkWindowTicks - LinkReactionTicks && s.SwapCooldown == 0
            && reserve == s.LinkHelper && s.Hp[reserve] * 100 / s.MaxHp[reserve] > 40)
        {
            buttons |= InputButtons.Swap;
        }

        if (fx == 0 && fy == 0)
        {
            return new InputFrame { Buttons = buttons };
        }
        // 64 направления, как у цифрового стика с диагоналями: меньше смен ввода — короче реплей.
        int heading = (FixMath.Atan2(fy, fx) + HeadingStep / 2) & -HeadingStep;
        return new InputFrame
        {
            MoveX = InputFrame.ClampAxis((int)((FixMath.Cos(heading).Raw * 127) >> Fix.FractionBits)),
            MoveY = InputFrame.ClampAxis((int)((FixMath.Sin(heading).Raw * 127) >> Fix.FractionBits)),
            Buttons = buttons,
        };
    }

    /// <summary>
    /// Босс жив и не замахивается: держать дистанцию ~75% дальности атаки героя, но не ближе круга удара + 20 px —
    /// автоатака бьёт босса приоритетно, а бегство от него затягивает акт до ярости. Ближний бой подходит вплотную
    /// до края босса: от удара его уводит телеграф (замах), а не дистанция.
    /// </summary>
    private static void EngageBoss(World world, EnemyKind boss, long dx, long dy, ref long fx, ref long fy)
    {
        long d = (long)FixMath.ISqrt((ulong)(dx * dx + dy * dy));
        HeroKind hero = world.ActiveHero;
        bool melee = hero.Pattern == AttackPattern.Cleave;
        long desired = melee ? (hero.RangePx + boss.RadiusPx).Raw * 75 / 100 : hero.RangePx.Raw * 75 / 100;
        long minimum = melee ? boss.RadiusPx.Raw : (boss.SlamRadiusPx + Fix.FromInt(20)).Raw;
        int angle = FixMath.Atan2(dy, dx);
        if (d > desired)
        {
            fx += FixMath.Cos(angle).Raw * 3;
            fy += FixMath.Sin(angle).Raw * 3;
        }
        else if (d < minimum)
        {
            fx -= FixMath.Cos(angle).Raw * 3;
            fy -= FixMath.Sin(angle).Raw * 3;
        }
    }

    /// <summary>Добавляет вектор «прочь от точки» (dx, dy — от героя к угрозе) с весом.</summary>
    private static void AddAway(ref long fx, ref long fy, long dx, long dy, int weight)
    {
        int angle = FixMath.Atan2(-dy, -dx);
        fx += FixMath.Cos(angle).Raw * weight;
        fy += FixMath.Sin(angle).Raw * weight;
    }

    /// <summary>
    /// Уклонение от опасной линии (луч из (ox, oy) по направлению (dirX, dirY) длиной length): если герой ближе halfWidth
    /// к лучу и впереди его начала — шаг вбок, в сторону, где он уже стоит. Возвращает true, если уклонялся.
    /// </summary>
    private static bool DodgeLine(long ox, long oy, long dirX, long dirY, long length, long halfWidth, long hx, long hy,
        ref long fx, ref long fy, bool directionIsVelocity = false)
    {
        if (directionIsVelocity)
        {
            int a = FixMath.Atan2(dirY, dirX);
            dirX = FixMath.Cos(a).Raw;
            dirY = FixMath.Sin(a).Raw;
        }
        long rx = hx - ox;
        long ry = hy - oy;
        long along = (rx * dirX + ry * dirY) >> Fix.FractionBits;
        if (along < 0 || along > length)
        {
            return false;
        }
        long side = (dirX * ry - dirY * rx) >> Fix.FractionBits;
        if (side > halfWidth || -side > halfWidth)
        {
            return false;
        }
        int sign = side >= 0 ? 1 : -1;
        fx += -dirY * sign * 6;
        fy += dirX * sign * 6;
        return true;
    }

    /// <summary>Ближнее нетронутое святилище в пределах поиска или −1; равные — меньший индекс.</summary>
    private static int NearestDormantShrine(World world, long hx, long hy)
    {
        PlacePool places = world.Places;
        long best = ShrineSearchRadius * ShrineSearchRadius;
        int found = -1;
        for (int i = 0; i < places.Count; i++)
        {
            if (places.Kind[i] != PlaceKind.Shrine || places.State[i] != PlaceState.Dormant)
            {
                continue;
            }
            long dx = places.X[i].Raw - hx;
            long dy = places.Y[i].Raw - hy;
            long d2 = dx * dx + dy * dy;
            if (d2 < best)
            {
                best = d2;
                found = i;
            }
        }
        return found;
    }

    private static int NearestGem(World world, long hx, long hy)
    {
        long best = GemSearchRadius * GemSearchRadius;
        int bestSlot = -1;
        var gems = world.Gems;
        for (int i = 0; i < gems.HighWater; i++)
        {
            if (!gems.Alive[i])
            {
                continue;
            }
            long dx = gems.X[i].Raw - hx;
            long dy = gems.Y[i].Raw - hy;
            long d2 = dx * dx + dy * dy;
            if (d2 < best)
            {
                best = d2;
                bestSlot = i;
            }
        }
        return bestSlot;
    }

    private static int PickBlessing(World world)
    {
        Squad s = world.Squad;
        int hpPercent = s.Hp[s.Active] * 100 / s.MaxHp[s.Active];
        int best = 0;
        int bestScore = int.MinValue;
        for (int k = 0; k < world.ModalOptionCount; k++)
        {
            int score = (Blessing)world.ModalOption(k) switch
            {
                Blessing.Might => 60,
                Blessing.Volley => 58,
                Blessing.Haste => 55,
                Blessing.Pierce => 45,
                Blessing.Vigor => 40,
                Blessing.Swift => 35,
                Blessing.Magnet => 30,
                Blessing.Mend => hpPercent < 50 ? 70 : 10,
                _ => 0,
            };
            if (score > bestScore)
            {
                bestScore = score;
                best = k;
            }
        }
        return best;
    }

    private static int PickCheckpoint(World world)
    {
        for (int k = 0; k < world.ModalOptionCount; k++)
        {
            if ((CheckpointOption)world.ModalOption(k) == CheckpointOption.Revive)
            {
                return k;
            }
        }
        Squad s = world.Squad;
        int hp = 0;
        int max = 0;
        for (int i = 0; i < Squad.Size; i++)
        {
            if (s.Alive[i])
            {
                hp += s.Hp[i];
                max += s.MaxHp[i];
            }
        }
        for (int k = 0; k < world.ModalOptionCount; k++)
        {
            var option = (CheckpointOption)world.ModalOption(k);
            if (option == (hp * 100 < max * 70 ? CheckpointOption.Heal : CheckpointOption.Might))
            {
                return k;
            }
        }
        return 0;
    }
}
