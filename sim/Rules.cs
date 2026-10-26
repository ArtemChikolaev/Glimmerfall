using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim;

/// <summary>
/// Формулы правил в одном месте: их зовут и сим, и UI (скилл sim-invariants, правило 5 — числа в подсказках
/// считаются теми же функциями). Проценты — целые, округление вниз.
/// </summary>
public static class Rules
{
    public const int ContactCooldownTicks = 30;
    public const int DashTicks = 10;
    public const int DashCooldownTicks = 150;
    public const int DashSpeedPercent = 350;
    public const int SwapCooldownTicks = 300;
    public const int SwapInvulnerableTicks = 30;
    public const int ForcedSwapInvulnerableTicks = 90;
    public const int CritPercent = 10;
    public const int ReserveRegenPeriodTicks = 30;
    public const int ReserveRegenPercent = 1;
    public const int BasePickupRadiusPx = 48;
    public const int CheckpointHealPercent = 60;
    public const int ReviveHpPercent = 50;
    public const int MendHealPercent = 35;
    public const int VigorHpPerLevel = 20;

    /// <summary>Ассист (PRD §5): откат общий на отряд; урон — доля урона атаки выпрыгнувшего героя.</summary>
    public const int AssistCooldownTicks = 8 * 60;
    public const int AssistDamagePercent = 300;
    /// <summary>Удар входящего героя при смене — по кругу отброса.</summary>
    public const int SwapStrikeDamagePercent = 200;
    public const int ChainLightningTargets = 8;

    /// <summary>
    /// Связка (PRD §5): после ассиста — окно, в котором свап на выпрыгнувшего героя запускает дуэт-приём пары.
    /// Урон дуэта — доля суммы уронов атаки обоих героев.
    /// </summary>
    public const int LinkWindowTicks = 2 * 60;
    public const int DuetDamagePercent = 400;
    public const int IceStormTargets = 10;
    public const int FirestormBlasts = 6;

    /// <summary>
    /// Места акта (PRD §4): святилища — постой в круге даёт силу на время; алтари риска — касание будит стража со
    /// свитой, убитый страж — лишний выбор благословения.
    /// </summary>
    public const int ShrinesPerAct = 3;
    public const int AltarsPerAct = 2;
    public const int ShrineChannelTicks = 2 * 60;
    public const int ShrineBuffTicks = 20 * 60;
    public const int ShrineDamagePercent = 40;
    public const int AltarEscort = 6;

    /// <summary>
    /// Преследователь встаёт в ChaseStopPx от касания с героем, а бьёт касанием в пределах ContactReachPx — досягаемость
    /// обязана быть больше остановки: иначе одиночный враг стоял вплотную и не наносил урона (находка ревью 2026-10-26).
    /// </summary>
    public static readonly Fix ChaseStopPx = Fix.FromInt(1);
    public static readonly Fix ContactReachPx = Fix.FromInt(2);
    public static readonly Fix KnockbackRadiusPx = Fix.FromInt(80);
    public static readonly Fix KnockbackDistancePx = Fix.FromInt(48);
    public static readonly Fix GemCollectRadiusPx = Fix.FromInt(12);
    public static readonly Fix GemMagnetSpeedPxPerTick = Fix.PerSecond(280);
    public static readonly Fix ShotRadiusPx = Fix.FromInt(4);
    public static readonly Fix MaxSeparationPushPx = Fix.Milli(1500);
    public static readonly Fix FireRingRadiusPx = Fix.FromInt(96);
    public static readonly Fix ShieldBashRadiusPx = Fix.FromInt(120);
    public static readonly Fix ShieldBashPushPx = Fix.FromInt(56);
    public static readonly Fix ChainFirstRangePx = Fix.FromInt(220);
    public static readonly Fix ChainHopPx = Fix.FromInt(130);
    public static readonly Fix SteamBurstRadiusPx = Fix.FromInt(150);
    public static readonly Fix SteamBurstPushPx = Fix.FromInt(96);
    public static readonly Fix IceStormRangePx = Fix.FromInt(260);
    public static readonly Fix FirestormRangePx = Fix.FromInt(240);
    public static readonly Fix FirestormBlastRadiusPx = Fix.FromInt(52);
    public static readonly Fix FirestormSpacingPx = Fix.FromInt(72);
    public static readonly Fix PlaceRadiusPx = Fix.FromInt(40);
    public static readonly Fix PlaceKeepOutPx = Fix.FromInt(520);
    public static readonly Fix PlaceSpacingPx = Fix.FromInt(600);
    public static readonly Fix AltarGuardianDistancePx = Fix.FromInt(130);
    public static readonly Fix AltarEscortRadiusPx = Fix.FromInt(170);

    /// <summary>Урон героя, пока действует сила святилища.</summary>
    public static int ShrineDamage(int damage) => Percent(damage, 100 + ShrineDamagePercent);

    /// <summary>Урон приёма ассиста — от урона атаки героя с «Мощью» и метой.</summary>
    public static int AssistDamage(HeroKind kind, int mightLevel, in MetaLoadout meta) =>
        Percent(HeroDamage(kind, mightLevel, meta), AssistDamagePercent);

    public static int SwapStrikeDamage(HeroKind kind, int mightLevel, in MetaLoadout meta) =>
        Percent(HeroDamage(kind, mightLevel, meta), SwapStrikeDamagePercent);

    /// <summary>Радиус приёма ассиста — для события и подсказки.</summary>
    public static Fix AssistRadius(AssistKind assist) => assist switch
    {
        AssistKind.FireRing => FireRingRadiusPx,
        AssistKind.ShieldBash => ShieldBashRadiusPx,
        _ => ChainFirstRangePx,
    };

    /// <summary>Урон дуэт-приёма — от суммы уронов атаки уходящего и входящего героев с «Мощью» и метой.</summary>
    public static int DuetDamage(HeroKind outgoing, HeroKind incoming, int mightLevel, in MetaLoadout meta) =>
        Percent(HeroDamage(outgoing, mightLevel, meta) + HeroDamage(incoming, mightLevel, meta), DuetDamagePercent);

    /// <summary>Дуэт пары по стихиям, порядок не важен; у пары одной стихии дуэта нет.</summary>
    public static bool TryDuet(Element a, Element b, out DuetKind duet)
    {
        duet = (a, b) switch
        {
            (Element.Fire, Element.Frost) or (Element.Frost, Element.Fire) => DuetKind.SteamBurst,
            (Element.Frost, Element.Lightning) or (Element.Lightning, Element.Frost) => DuetKind.IceStorm,
            _ => DuetKind.Firestorm,
        };
        return a != b;
    }

    /// <summary>Радиус дуэт-приёма (досягаемость) — для события и подсказки.</summary>
    public static Fix DuetRadius(DuetKind duet) => duet switch
    {
        DuetKind.SteamBurst => SteamBurstRadiusPx,
        DuetKind.IceStorm => IceStormRangePx,
        _ => FirestormRangePx,
    };

    public static int Percent(int value, int percent) => (int)((long)value * percent / 100);

    public static int HeroDamage(HeroKind kind, int mightLevel, in MetaLoadout meta)
    {
        int damage = Percent(kind.Damage, 100 + 15 * mightLevel + 5 * meta.Damage);
        return damage < 1 ? 1 : damage;
    }

    public static int AttackInterval(HeroKind kind, int hasteLevel)
    {
        int interval = Percent(kind.AttackIntervalTicks, 100 - 8 * hasteLevel);
        return interval < 6 ? 6 : interval;
    }

    public static int ProjectileCount(HeroKind kind, int volleyLevel) =>
        kind.ProjectileCount + (kind.Pattern == AttackPattern.Nova ? 2 * volleyLevel : volleyLevel);

    /// <summary>Урон по герою после брони; не меньше единицы, если урон был.</summary>
    public static int DamageTaken(HeroKind kind, int amount)
    {
        int taken = Percent(amount, 100 - kind.ArmorPercent);
        return taken < 1 && amount > 0 ? 1 : taken;
    }

    /// <summary>Пробитие снаряда; у удара дугой — сколько врагов задевает один удар.</summary>
    public static int Pierce(HeroKind kind, int pierceLevel) => kind.Pierce + pierceLevel;

    /// <summary>Полуугол дуги удара в ближнем бою: «Залп» расширяет замах на 10° за уровень, не шире 120° в сторону.</summary>
    public static int CleaveArc(HeroKind kind, int volleyLevel)
    {
        int arc = kind.SpreadAngle + FixMath.Degrees(10) * volleyLevel;
        int max = FixMath.Degrees(120);
        return arc < max ? arc : max;
    }

    public static Fix MoveSpeed(HeroKind kind, int swiftLevel, in MetaLoadout meta) =>
        kind.SpeedPxPerTick * (100 + 8 * swiftLevel + 3 * meta.Speed) / 100;

    public static int HeroMaxHp(HeroKind kind, int vigorLevel, in MetaLoadout meta) =>
        Percent(kind.MaxHp, 100 + 8 * meta.Health) + VigorHpPerLevel * vigorLevel;

    public static Fix PickupRadius(int magnetLevel, in MetaLoadout meta) =>
        Fix.FromInt(Percent(BasePickupRadiusPx, 100 + 30 * magnetLevel + 10 * meta.Magnet));

    /// <summary>Здоровье врага с учётом тира лестницы и номера акта (0 — первый).</summary>
    public static int EnemyHp(EnemyKind kind, int tier, int actIndex) =>
        Percent(Percent(kind.Hp, 100 + 30 * tier), 100 + 70 * actIndex);

    public static int EnemyDamage(int baseDamage, int tier, int actIndex) =>
        Percent(Percent(baseDamage, 100 + 15 * tier), 100 + 25 * actIndex);

    /// <summary>Темп спавна, тысячные доли врага в секунду.</summary>
    public static int SpawnRateMilli(SimConfig config, int actIndex, int actTick)
    {
        long span = config.SpawnRateEndMilli - config.SpawnRateStartMilli;
        int t = actTick < config.ActTicks ? actTick : config.ActTicks;
        int rate = config.SpawnRateStartMilli + (int)(span * t / config.ActTicks);
        rate = Percent(rate, 100 + config.SpawnPerActPercent * actIndex);
        return Percent(rate, 100 + 10 * config.Tier);
    }

    /// <summary>Опыт до следующего уровня.</summary>
    public static int XpToNext(int level) => 10 + 5 * (level - 1);

    /// <summary>
    /// Очки забега для таблиц рекордов: убийства + 5000 за закрытый акт. Сервер не верит очкам клиента — их считает
    /// валидатор этой же функцией после ре-симуляции реплея.
    /// </summary>
    public static int RunScore(World world)
    {
        int cleared = 0;
        foreach (Core.ActStats act in world.Stats.Acts)
        {
            if (act.Cleared)
            {
                cleared++;
            }
        }
        return world.Stats.TotalKills + 5000 * cleared;
    }
}
