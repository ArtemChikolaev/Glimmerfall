using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Numerics;

namespace Glimmerfall.Sim.Core;

/// <summary>
/// Слоты фиксированной ёмкости со стеком свободных (LIFO) и поколениями. Порядок выдачи слотов детерминирован
/// и входит в хеш состояния. Освобождение — только в конце тика (<see cref="World"/>), чтобы слот умирающего
/// не достался новичку посреди тика (скилл sim-invariants, правило 2).
/// </summary>
public abstract class SlotPool
{
    public readonly int Capacity;
    public readonly bool[] Alive;
    public readonly ushort[] Generation;

    /// <summary>Живые слоты лежат в [0, HighWater).</summary>
    public int HighWater { get; private set; }
    public int Count { get; private set; }

    private readonly int[] _free;
    private int _freeCount;

    protected SlotPool(int capacity)
    {
        Capacity = capacity;
        Alive = new bool[capacity];
        Generation = new ushort[capacity];
        _free = new int[capacity];
    }

    /// <summary>Новый слот или −1, если пул полон.</summary>
    protected int AllocateSlot()
    {
        int slot;
        if (_freeCount > 0)
        {
            slot = _free[--_freeCount];
        }
        else if (HighWater < Capacity)
        {
            slot = HighWater++;
        }
        else
        {
            return -1;
        }
        Alive[slot] = true;
        Generation[slot]++;
        Count++;
        return slot;
    }

    /// <summary>Освобождает слот. Вызывающий отвечает за то, что слот уже не жив логически (Alive снят раньше).</summary>
    public void Release(int slot)
    {
        Alive[slot] = false;
        _free[_freeCount++] = slot;
        Count--;
    }

    public void Clear()
    {
        for (int i = 0; i < HighWater; i++)
        {
            Alive[i] = false;
        }
        HighWater = 0;
        Count = 0;
        _freeCount = 0;
    }

    /// <summary>Ссылка на сущность, переживающая тик: слот и поколение. Устаревшая ссылка не «оживает» в новичке.</summary>
    public int Handle(int slot) => slot | (Generation[slot] << 16);

    public bool IsLive(int handle)
    {
        if (handle < 0)
        {
            return false;
        }
        int slot = handle & 0xFFFF;
        return slot < Capacity && Alive[slot] && Generation[slot] == (ushort)(handle >> 16);
    }

    protected void AddPoolTo(ref StateHasher hasher)
    {
        hasher.Add(HighWater);
        hasher.Add(Count);
        hasher.Add(_freeCount);
        for (int i = 0; i < _freeCount; i++)
        {
            hasher.Add(_free[i]);
        }
    }
}

public enum EnemyState : byte
{
    Normal = 0,
    /// <summary>Телеграф: стрелок целится, таран замахивается, босс заносит удар.</summary>
    Telegraph = 1,
    Charging = 2,
}

/// <summary>Враги — структура массивов (SoA). PrevX/PrevY и HitFlash — подсказки презентации, в хеш не входят.</summary>
public sealed class EnemyPool : SlotPool
{
    public readonly byte[] Kind;
    public readonly Fix[] X;
    public readonly Fix[] Y;
    public readonly Fix[] PrevX;
    public readonly Fix[] PrevY;
    public readonly Fix[] Radius;
    public readonly int[] Hp;
    public readonly int[] MaxHp;
    public readonly EnemyState[] State;
    public readonly int[] StateTicks;
    public readonly int[] Cooldown;
    public readonly int[] ContactCooldown;
    public readonly Fix[] DirX;
    public readonly Fix[] DirY;
    public readonly int[] HitFlash;

    public EnemyPool(int capacity) : base(capacity)
    {
        Kind = new byte[capacity];
        X = new Fix[capacity];
        Y = new Fix[capacity];
        PrevX = new Fix[capacity];
        PrevY = new Fix[capacity];
        Radius = new Fix[capacity];
        Hp = new int[capacity];
        MaxHp = new int[capacity];
        State = new EnemyState[capacity];
        StateTicks = new int[capacity];
        Cooldown = new int[capacity];
        ContactCooldown = new int[capacity];
        DirX = new Fix[capacity];
        DirY = new Fix[capacity];
        HitFlash = new int[capacity];
    }

    public int Spawn(int kind, Fix x, Fix y, Fix radius, int hp, int initialCooldown)
    {
        int slot = AllocateSlot();
        if (slot < 0)
        {
            return -1;
        }
        Kind[slot] = (byte)kind;
        X[slot] = x;
        Y[slot] = y;
        PrevX[slot] = x;
        PrevY[slot] = y;
        Radius[slot] = radius;
        Hp[slot] = hp;
        MaxHp[slot] = hp;
        State[slot] = EnemyState.Normal;
        StateTicks[slot] = 0;
        Cooldown[slot] = initialCooldown;
        ContactCooldown[slot] = 0;
        DirX[slot] = Fix.Zero;
        DirY[slot] = Fix.Zero;
        HitFlash[slot] = 0;
        return slot;
    }

    public void AddTo(ref StateHasher hasher)
    {
        AddPoolTo(ref hasher);
        for (int i = 0; i < HighWater; i++)
        {
            if (!Alive[i])
            {
                continue;
            }
            hasher.Add(i);
            hasher.Add(Generation[i]);
            hasher.Add(Kind[i]);
            hasher.Add(X[i].Raw);
            hasher.Add(Y[i].Raw);
            hasher.Add(Hp[i]);
            hasher.Add((int)State[i]);
            hasher.Add(StateTicks[i]);
            hasher.Add(Cooldown[i]);
            hasher.Add(ContactCooldown[i]);
            hasher.Add(DirX[i].Raw);
            hasher.Add(DirY[i].Raw);
        }
    }
}

/// <summary>Снаряды отряда.</summary>
public sealed class ProjectilePool : SlotPool
{
    public readonly Fix[] X;
    public readonly Fix[] Y;
    public readonly Fix[] PrevX;
    public readonly Fix[] PrevY;
    public readonly Fix[] VX;
    public readonly Fix[] VY;
    public readonly Fix[] Radius;
    public readonly int[] Life;
    public readonly int[] Damage;
    public readonly int[] Pierce;
    /// <summary>Хендл последнего задетого врага: пробивающий снаряд не бьёт одного врага каждый тик.</summary>
    public readonly int[] LastHit;
    public readonly byte[] HeroKind;

    public ProjectilePool(int capacity) : base(capacity)
    {
        X = new Fix[capacity];
        Y = new Fix[capacity];
        PrevX = new Fix[capacity];
        PrevY = new Fix[capacity];
        VX = new Fix[capacity];
        VY = new Fix[capacity];
        Radius = new Fix[capacity];
        Life = new int[capacity];
        Damage = new int[capacity];
        Pierce = new int[capacity];
        LastHit = new int[capacity];
        HeroKind = new byte[capacity];
    }

    public int Spawn(Fix x, Fix y, Fix vx, Fix vy, Fix radius, int life, int damage, int pierce, int heroKind)
    {
        int slot = AllocateSlot();
        if (slot < 0)
        {
            return -1;
        }
        X[slot] = x;
        Y[slot] = y;
        PrevX[slot] = x;
        PrevY[slot] = y;
        VX[slot] = vx;
        VY[slot] = vy;
        Radius[slot] = radius;
        Life[slot] = life;
        Damage[slot] = damage;
        Pierce[slot] = pierce;
        LastHit[slot] = -1;
        HeroKind[slot] = (byte)heroKind;
        return slot;
    }

    public void AddTo(ref StateHasher hasher)
    {
        AddPoolTo(ref hasher);
        for (int i = 0; i < HighWater; i++)
        {
            if (!Alive[i])
            {
                continue;
            }
            hasher.Add(i);
            hasher.Add(X[i].Raw);
            hasher.Add(Y[i].Raw);
            hasher.Add(VX[i].Raw);
            hasher.Add(VY[i].Raw);
            hasher.Add(Life[i]);
            hasher.Add(Damage[i]);
            hasher.Add(Pierce[i]);
            hasher.Add(LastHit[i]);
        }
    }
}

/// <summary>Снаряды врагов (летят только в отряд).</summary>
public sealed class ShotPool : SlotPool
{
    public readonly Fix[] X;
    public readonly Fix[] Y;
    public readonly Fix[] PrevX;
    public readonly Fix[] PrevY;
    public readonly Fix[] VX;
    public readonly Fix[] VY;
    public readonly Fix[] Radius;
    public readonly int[] Life;
    public readonly int[] Damage;
    public readonly byte[] SourceKind;

    public ShotPool(int capacity) : base(capacity)
    {
        X = new Fix[capacity];
        Y = new Fix[capacity];
        PrevX = new Fix[capacity];
        PrevY = new Fix[capacity];
        VX = new Fix[capacity];
        VY = new Fix[capacity];
        Radius = new Fix[capacity];
        Life = new int[capacity];
        Damage = new int[capacity];
        SourceKind = new byte[capacity];
    }

    public int Spawn(Fix x, Fix y, Fix vx, Fix vy, Fix radius, int life, int damage, int sourceKind)
    {
        int slot = AllocateSlot();
        if (slot < 0)
        {
            return -1;
        }
        X[slot] = x;
        Y[slot] = y;
        PrevX[slot] = x;
        PrevY[slot] = y;
        VX[slot] = vx;
        VY[slot] = vy;
        Radius[slot] = radius;
        Life[slot] = life;
        Damage[slot] = damage;
        SourceKind[slot] = (byte)sourceKind;
        return slot;
    }

    public void AddTo(ref StateHasher hasher)
    {
        AddPoolTo(ref hasher);
        for (int i = 0; i < HighWater; i++)
        {
            if (!Alive[i])
            {
                continue;
            }
            hasher.Add(i);
            hasher.Add(X[i].Raw);
            hasher.Add(Y[i].Raw);
            hasher.Add(VX[i].Raw);
            hasher.Add(VY[i].Raw);
            hasher.Add(Life[i]);
            hasher.Add(Damage[i]);
            hasher.Add(SourceKind[i]);
        }
    }
}

/// <summary>Сферы опыта.</summary>
public sealed class GemPool : SlotPool
{
    public readonly Fix[] X;
    public readonly Fix[] Y;
    public readonly Fix[] PrevX;
    public readonly Fix[] PrevY;
    public readonly int[] Value;
    public readonly bool[] Magnetized;

    public GemPool(int capacity) : base(capacity)
    {
        X = new Fix[capacity];
        Y = new Fix[capacity];
        PrevX = new Fix[capacity];
        PrevY = new Fix[capacity];
        Value = new int[capacity];
        Magnetized = new bool[capacity];
    }

    public int Spawn(Fix x, Fix y, int value)
    {
        int slot = AllocateSlot();
        if (slot < 0)
        {
            return -1;
        }
        X[slot] = x;
        Y[slot] = y;
        PrevX[slot] = x;
        PrevY[slot] = y;
        Value[slot] = value;
        Magnetized[slot] = false;
        return slot;
    }

    public void AddTo(ref StateHasher hasher)
    {
        AddPoolTo(ref hasher);
        for (int i = 0; i < HighWater; i++)
        {
            if (!Alive[i])
            {
                continue;
            }
            hasher.Add(i);
            hasher.Add(X[i].Raw);
            hasher.Add(Y[i].Raw);
            hasher.Add(Value[i]);
            hasher.Add(Magnetized[i]);
        }
    }
}
