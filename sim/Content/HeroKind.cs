using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Content;

public enum AttackPattern : byte
{
    /// <summary>Одиночный снаряд в ближайшего врага.</summary>
    Bolt = 0,
    /// <summary>Веер снарядов в ближайшего врага.</summary>
    Fan = 1,
    /// <summary>Кольцо снарядов вокруг героя, если враг в радиусе.</summary>
    Nova = 2,
}

/// <summary>Вид героя отряда. Единицы — в именах полей. Создаётся заново для каждого конфига (копия на забег).</summary>
public sealed class HeroKind
{
    public int Id;
    public string Name = "";
    public int MaxHp;
    public Fix SpeedPxPerTick;
    public Fix RadiusPx;
    public AttackPattern Pattern;
    public int AttackIntervalTicks;
    public int ProjectileCount;
    public int SpreadAngle;
    public int Damage;
    public Fix ProjectileSpeedPxPerTick;
    public Fix RangePx;
    public int Pierce;
    public Fix ProjectileRadiusPx;

    public HeroKind Clone() => (HeroKind)MemberwiseClone();
}
