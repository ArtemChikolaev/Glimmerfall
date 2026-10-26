using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Core;

/// <summary>Вид места на карте акта (PRD §4; скилл encounter-design: «места» спят до контакта).</summary>
public enum PlaceKind : byte
{
    /// <summary>Святилище: постой в круге — временное усиление урона отряда.</summary>
    Shrine = 0,
    /// <summary>Алтарь риска: касание будит элиту со свитой; убил элиту — дар (лишний выбор благословения).</summary>
    Altar = 1,
}

public enum PlaceState : byte
{
    Dormant = 0,
    /// <summary>Алтарь разбужен: его страж жив.</summary>
    Awake = 1,
    Spent = 2,
}

/// <summary>
/// Места акта — состояние сима: расставляются от сида (поток Places) в начале акта, прогресс и исход — в дайджесте.
/// Презентация читает их каждый кадр: реквизит, свет, кольцо прогресса, метки на краю экрана.
/// </summary>
public sealed class PlacePool
{
    public const int Capacity = 8;

    public readonly PlaceKind[] Kind = new PlaceKind[Capacity];
    public readonly Fix[] X = new Fix[Capacity];
    public readonly Fix[] Y = new Fix[Capacity];
    public readonly PlaceState[] State = new PlaceState[Capacity];

    /// <summary>Тики стояния в святилище: растут в круге, тают вне его.</summary>
    public readonly int[] Progress = new int[Capacity];

    /// <summary>Хендл элиты, разбуженной алтарём; −1 — нет (ещё не появилась или не алтарь).</summary>
    public readonly int[] Guardian = new int[Capacity];

    public int Count;

    public void Clear() => Count = 0;

    public void Add(PlaceKind kind, Fix x, Fix y)
    {
        if (Count == Capacity)
        {
            return;
        }
        int i = Count++;
        Kind[i] = kind;
        X[i] = x;
        Y[i] = y;
        State[i] = PlaceState.Dormant;
        Progress[i] = 0;
        Guardian[i] = -1;
    }

    public void AddTo(ref StateHasher hasher)
    {
        hasher.Add(Count);
        for (int i = 0; i < Count; i++)
        {
            hasher.Add((int)Kind[i]);
            hasher.Add(X[i].Raw);
            hasher.Add(Y[i].Raw);
            hasher.Add((int)State[i]);
            hasher.Add(Progress[i]);
            hasher.Add(Guardian[i]);
        }
    }
}
