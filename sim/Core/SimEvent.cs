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
}
