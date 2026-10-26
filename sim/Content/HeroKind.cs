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
    /// <summary>
    /// Ближний бой: удар дугой по всем врагам в секторе перед героем (ADR 0003, уточнение 2026-10-26). RangePx —
    /// досягаемость, SpreadAngle — полуугол дуги, Pierce — сколько врагов задевает один удар, CleavePushPx — отброс.
    /// </summary>
    Cleave = 3,
}

/// <summary>Фирменный приём ассиста (PRD §5): резервный герой выпрыгивает, бьёт и уходит.</summary>
public enum AssistKind : byte
{
    /// <summary>Огненное кольцо вокруг отряда.</summary>
    FireRing = 0,
    /// <summary>Удар щитом конусом по направлению взгляда с отбросом рядовых.</summary>
    ShieldBash = 1,
    /// <summary>Цепная молния: ближайший враг, затем прыжки к следующим.</summary>
    ChainLightning = 2,
}

/// <summary>Стихия героя: по паре стихий выбирается дуэт-приём связки.</summary>
public enum Element : byte
{
    Fire = 0,
    Frost = 1,
    Lightning = 2,
}

/// <summary>Дуэт-приём связки (PRD §5): смесь стихий пары, которая сменяется на поле.</summary>
public enum DuetKind : byte
{
    /// <summary>Огонь + мороз: паровой взрыв вокруг отряда, сильный отброс рядовых.</summary>
    SteamBurst = 0,
    /// <summary>Мороз + молния: удары с неба, сначала по элитам и боссу, затем по ближайшим.</summary>
    IceStorm = 1,
    /// <summary>Молния + огонь: взрывы в нескольких точках вокруг отряда, от ближних к дальним.</summary>
    Firestorm = 2,
}

/// <summary>Вид героя отряда. Единицы — в именах полей. Создаётся заново для каждого конфига (копия на забег).</summary>
public sealed class HeroKind
{
    public int Id;
    public string Name = "";
    public int MaxHp;
    public Fix SpeedPxPerTick;
    public Fix RadiusPx;
    public Element Element;
    public AssistKind Assist;
    public AttackPattern Pattern;
    public int AttackIntervalTicks;
    public int ProjectileCount;
    public int SpreadAngle;
    public int Damage;
    public Fix ProjectileSpeedPxPerTick;
    public Fix RangePx;
    public int Pierce;
    public Fix ProjectileRadiusPx;
    /// <summary>Отброс рядовых ударом дугой (тяжёлый удар); ноль — без отброса.</summary>
    public Fix CleavePushPx;
    /// <summary>Броня: на сколько процентов меньше урона получает герой. Ближнему бою — плата за жизнь вплотную к орде.</summary>
    public int ArmorPercent;

    public HeroKind Clone() => (HeroKind)MemberwiseClone();
}
