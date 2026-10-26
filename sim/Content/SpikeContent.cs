using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Content;

/// <summary>Индексы видов врагов спайка (= <see cref="EnemyKind.Id"/>).</summary>
public static class EnemyIds
{
    public const int Shambler = 0;
    public const int Runner = 1;
    public const int Brute = 2;
    public const int Spitter = 3;
    public const int Charger = 4;
    public const int Ravager = 5;
    public const int GloomWarden = 6;
    public const int Count = 7;
}

/// <summary>Индексы героев спайка (= <see cref="HeroKind.Id"/>).</summary>
public static class HeroIds
{
    public const int Lamplighter = 0;
    public const int Warden = 1;
    public const int Sparkwright = 2;
    public const int Count = 3;
}

/// <summary>
/// Данные спайка этапа 1 в коде. Рабочий контент переедет в content/ под JSON Schema (ADR 0002) вместе с вертикальным срезом;
/// здесь — минимальный набор, на котором меряются орда, детерминизм и бот: рой, бегуны, громилы, стрелки, тараны, элита и босс.
/// </summary>
public static class SpikeContent
{
    public static EnemyKind[] CreateEnemyKinds() => new[]
    {
        new EnemyKind
        {
            Id = EnemyIds.Shambler, Name = "shambler", Behavior = EnemyBehavior.Chase,
            RadiusPx = Fix.FromInt(7), SpeedPxPerTick = Fix.PerSecond(32), Hp = 14, ContactDamage = 7, Xp = 1,
        },
        new EnemyKind
        {
            Id = EnemyIds.Runner, Name = "runner", Behavior = EnemyBehavior.Chase,
            RadiusPx = Fix.FromInt(6), SpeedPxPerTick = Fix.PerSecond(58), Hp = 9, ContactDamage = 5, Xp = 1,
        },
        new EnemyKind
        {
            Id = EnemyIds.Brute, Name = "brute", Behavior = EnemyBehavior.Chase,
            RadiusPx = Fix.FromInt(12), SpeedPxPerTick = Fix.PerSecond(24), Hp = 80, ContactDamage = 16, Xp = 4,
        },
        new EnemyKind
        {
            Id = EnemyIds.Spitter, Name = "spitter", Behavior = EnemyBehavior.Ranged,
            RadiusPx = Fix.FromInt(7), SpeedPxPerTick = Fix.PerSecond(30), Hp = 18, ContactDamage = 4, Xp = 2,
            PreferredRangePx = Fix.FromInt(160), AimTicks = 45, FireCooldownTicks = 150,
            ShotSpeedPxPerTick = Fix.PerSecond(150), ShotDamage = 9,
        },
        new EnemyKind
        {
            Id = EnemyIds.Charger, Name = "charger", Behavior = EnemyBehavior.Charger,
            RadiusPx = Fix.FromInt(9), SpeedPxPerTick = Fix.PerSecond(34), Hp = 45, ContactDamage = 10, Xp = 3,
            ChargeTriggerPx = Fix.FromInt(190), ChargeWindupTicks = 45, ChargeTicks = 36,
            ChargeSpeedPxPerTick = Fix.PerSecond(240), ChargeCooldownTicks = 240,
        },
        new EnemyKind
        {
            Id = EnemyIds.Ravager, Name = "ravager", Behavior = EnemyBehavior.Chase, Elite = true,
            RadiusPx = Fix.FromInt(14), SpeedPxPerTick = Fix.PerSecond(44), Hp = 450, ContactDamage = 18, Xp = 30,
        },
        new EnemyKind
        {
            Id = EnemyIds.GloomWarden, Name = "gloom_warden", Behavior = EnemyBehavior.Boss, Elite = true,
            RadiusPx = Fix.FromInt(26), SpeedPxPerTick = Fix.PerSecond(30), Hp = 5000, ContactDamage = 28, Xp = 0,
            SlamRadiusPx = Fix.FromInt(96), SlamWindupTicks = 70, SlamCooldownTicks = 300, SlamDamage = 38,
        },
    };

    public static HeroKind[] CreateHeroKinds() => new[]
    {
        new HeroKind
        {
            Id = HeroIds.Lamplighter, Name = "lamplighter", MaxHp = 100, SpeedPxPerTick = Fix.PerSecond(92), RadiusPx = Fix.FromInt(8),
            Assist = AssistKind.FireRing, Pattern = AttackPattern.Bolt, AttackIntervalTicks = 34, ProjectileCount = 1, SpreadAngle = 0, Damage = 11,
            ProjectileSpeedPxPerTick = Fix.PerSecond(320), RangePx = Fix.FromInt(270), Pierce = 1, ProjectileRadiusPx = Fix.FromInt(4),
        },
        new HeroKind
        {
            Id = HeroIds.Warden, Name = "warden", MaxHp = 140, SpeedPxPerTick = Fix.PerSecond(84), RadiusPx = Fix.FromInt(9),
            Assist = AssistKind.ShieldBash, Pattern = AttackPattern.Fan, AttackIntervalTicks = 46, ProjectileCount = 3, SpreadAngle = FixMath.Degrees(12), Damage = 8,
            ProjectileSpeedPxPerTick = Fix.PerSecond(260), RangePx = Fix.FromInt(210), Pierce = 2, ProjectileRadiusPx = Fix.FromInt(4),
        },
        new HeroKind
        {
            Id = HeroIds.Sparkwright, Name = "sparkwright", MaxHp = 90, SpeedPxPerTick = Fix.PerSecond(96), RadiusPx = Fix.FromInt(8),
            Assist = AssistKind.ChainLightning, Pattern = AttackPattern.Nova, AttackIntervalTicks = 70, ProjectileCount = 10, SpreadAngle = 0, Damage = 7,
            ProjectileSpeedPxPerTick = Fix.PerSecond(210), RangePx = Fix.FromInt(190), Pierce = 1, ProjectileRadiusPx = Fix.FromInt(3),
        },
    };
}
