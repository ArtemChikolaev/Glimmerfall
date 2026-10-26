using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Content;

public enum EnemyBehavior : byte
{
    Chase = 0,
    Ranged = 1,
    Charger = 2,
    Boss = 3,
}

/// <summary>
/// Вид врага. Единицы — в именах полей: пиксели базовой сетки, тики (60 Гц), пиксели за тик.
/// Экземпляры создаёт <see cref="SpikeContent"/> заново для каждого конфига — правка одного забега не протекает в другой.
/// </summary>
public sealed class EnemyKind
{
    public int Id;
    public string Name = "";
    public EnemyBehavior Behavior;
    public bool Elite;
    public Fix RadiusPx;
    public Fix SpeedPxPerTick;
    public int Hp;
    public int ContactDamage;
    public int Xp;

    /// <summary>Стрелок: держит дистанцию, целится (телеграф) и стреляет по зафиксированному направлению.</summary>
    public Fix PreferredRangePx;
    public int AimTicks;
    public int FireCooldownTicks;
    public Fix ShotSpeedPxPerTick;
    public int ShotDamage;

    /// <summary>Таран: замах на месте (телеграф линии), затем рывок по зафиксированному направлению.</summary>
    public Fix ChargeTriggerPx;
    public int ChargeWindupTicks;
    public int ChargeTicks;
    public Fix ChargeSpeedPxPerTick;
    public int ChargeCooldownTicks;

    /// <summary>Босс: удар по кругу вокруг себя после телеграфа.</summary>
    public Fix SlamRadiusPx;
    public int SlamWindupTicks;
    public int SlamCooldownTicks;
    public int SlamDamage;

    public EnemyKind Clone() => (EnemyKind)MemberwiseClone();
}
