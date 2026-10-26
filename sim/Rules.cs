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

    public static readonly Fix KnockbackRadiusPx = Fix.FromInt(80);
    public static readonly Fix KnockbackDistancePx = Fix.FromInt(48);
    public static readonly Fix GemCollectRadiusPx = Fix.FromInt(12);
    public static readonly Fix GemMagnetSpeedPxPerTick = Fix.PerSecond(280);
    public static readonly Fix ShotRadiusPx = Fix.FromInt(4);
    public static readonly Fix MaxSeparationPushPx = Fix.Milli(1500);

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

    public static int Pierce(HeroKind kind, int pierceLevel) => kind.Pierce + pierceLevel;

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
