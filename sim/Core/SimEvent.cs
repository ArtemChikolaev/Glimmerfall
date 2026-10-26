using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Core;

public enum SimEventType : byte
{
    EnemyHit = 0,
    EnemyKilled = 1,
    HeroHit = 2,
    HeroDown = 3,
    Swap = 4,
    Dash = 5,
    LevelUp = 6,
    Slam = 7,
    EnemyFired = 8,
    /// <summary>Атака активного героя: Kind — вид героя, Dir — направление на цель (у новы — первый луч).</summary>
    HeroAttack = 9,
    /// <summary>Ассист: Kind — вид выпрыгнувшего героя, Dir — взгляд отряда, Amount — радиус приёма.</summary>
    Assist = 10,
    /// <summary>Звено цепной молнии: из (X, Y) на Dir (вектор до цели); Kind — вид героя.</summary>
    ChainLink = 11,
    /// <summary>Удар входящего героя при смене: Kind — вид героя, Amount — радиус.</summary>
    SwapStrike = 12,
    /// <summary>
    /// Дуэт-приём связки: Kind — <see cref="Content.DuetKind"/>, Amount — досягаемость, Dir — взгляд отряда;
    /// Source — вид входящего героя, Target — вид уходящего (у этого события — герой, а не враг).
    /// </summary>
    Duet = 13,
    /// <summary>Удар дуэта в точке (X, Y): молния ледяной грозы (Amount 0) или взрыв огненного шторма (Amount — радиус).</summary>
    DuetStrike = 14,
}

/// <summary>
/// Событие тика для презентации (вспышки, частицы, звук). Сим пишет события, только если
/// <see cref="World.CollectEvents"/> включён; на логику и хеш они не влияют.
/// </summary>
public struct SimEvent
{
    public SimEventType Type;
    public Fix X;
    public Fix Y;
    /// <summary>Вид врага или героя, если применимо.</summary>
    public int Kind;
    public int Amount;

    /// <summary>Направление удара или атаки (скорость снаряда, единичный вектор цели); ноль — направления нет.</summary>
    public Fix DirX;
    public Fix DirY;

    /// <summary>Критическое попадание (урон удвоен).</summary>
    public bool Crit;

    /// <summary>Хендл задетого врага (для отклика на конкретной фигуре); −1 — нет.</summary>
    public int Target;

    /// <summary>Вид героя, чей снаряд попал (стихия искр); −1 — не герой.</summary>
    public int Source;
}
