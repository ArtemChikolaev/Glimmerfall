using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Частицы презентации (искры попаданий, шлейфы снарядов, пепел смертей). Живут только в game/: в сим и сид не
/// входят, поэтому случайность здесь своя и не детерминированная — это косметика.
/// </summary>
public sealed class Particles
{
    private const int Capacity = 8000;

    private readonly float[] _x = new float[Capacity];
    private readonly float[] _y = new float[Capacity];
    private readonly float[] _vx = new float[Capacity];
    private readonly float[] _vy = new float[Capacity];
    private readonly float[] _life = new float[Capacity];
    private readonly float[] _maxLife = new float[Capacity];
    private readonly float[] _size = new float[Capacity];
    private readonly float[] _rise = new float[Capacity];
    private readonly Color[] _color = new Color[Capacity];
    private readonly Random _random;
    private int _count;

    /// <summary>seed — только для сравнимых скриншотов (G2.6); в игре разброс частиц свой на каждый запуск.</summary>
    public Particles(int? seed = null)
    {
        _random = seed is int value ? new Random(value) : new Random();
    }

    public int Count => _count;

    public float Random01() => (float)_random.NextDouble();

    /// <summary>Вспышка во все стороны.</summary>
    public void Burst(float x, float y, int n, Color color, float speed, float life, float size, float rise = 0f)
    {
        for (int k = 0; k < n; k++)
        {
            Emit(x, y, (float)(_random.NextDouble() * Math.Tau), speed * (0.35f + 0.65f * Random01()), color, life, size, rise);
        }
    }

    /// <summary>Искры конусом по направлению удара.</summary>
    public void Spray(float x, float y, int n, Color color, float dirX, float dirY, float spread, float speed, float life, float size)
    {
        float baseAngle = MathF.Atan2(dirY, dirX);
        for (int k = 0; k < n; k++)
        {
            float angle = baseAngle + (Random01() * 2 - 1) * spread;
            Emit(x, y, angle, speed * (0.4f + 0.6f * Random01()), color, life, size, 0f);
        }
    }

    /// <summary>Одна медленная частица шлейфа с лёгким сносом.</summary>
    public void Trail(float x, float y, Color color, float life, float size, float drift = 12f, float rise = 0f)
    {
        Emit(x + (Random01() * 2 - 1) * 1.5f, y + (Random01() * 2 - 1) * 1.5f, (float)(_random.NextDouble() * Math.Tau), drift * Random01(), color, life, size, rise);
    }

    private void Emit(float x, float y, float angle, float speed, Color color, float life, float size, float rise)
    {
        if (_count >= Capacity)
        {
            return;
        }
        int i = _count++;
        _x[i] = x;
        _y[i] = y;
        _vx[i] = MathF.Cos(angle) * speed;
        _vy[i] = MathF.Sin(angle) * speed;
        _maxLife[i] = life * (0.6f + 0.4f * Random01());
        _life[i] = _maxLife[i];
        _size[i] = size;
        _rise[i] = rise;
        _color[i] = color;
    }

    public void Update(float dt)
    {
        float drag = MathF.Exp(-4f * dt);
        for (int i = 0; i < _count; i++)
        {
            _life[i] -= dt;
            if (_life[i] <= 0)
            {
                _count--;
                _x[i] = _x[_count];
                _y[i] = _y[_count];
                _vx[i] = _vx[_count];
                _vy[i] = _vy[_count];
                _life[i] = _life[_count];
                _maxLife[i] = _maxLife[_count];
                _size[i] = _size[_count];
                _rise[i] = _rise[_count];
                _color[i] = _color[_count];
                i--;
                continue;
            }
            _x[i] += _vx[i] * dt;
            _y[i] += (_vy[i] - _rise[i]) * dt;
            _vx[i] *= drag;
            _vy[i] *= drag;
        }
    }

    public void Draw(InstanceLayer layer)
    {
        for (int i = 0; i < _count; i++)
        {
            float t = _life[i] / _maxLife[i];
            Color c = _color[i];
            c.A *= t;
            layer.Add(_x[i], _y[i], _size[i] * (0.4f + 0.6f * t), c);
        }
    }
}

/// <summary>Расходящиеся кольца: смерть элиты, уровень, удар босса, вход героя.</summary>
public sealed class Rings
{
    private const int Capacity = 128;

    private readonly Ring[] _rings = new Ring[Capacity];
    private int _count;

    public void Add(float x, float y, float from, float to, float life, Color color)
    {
        if (_count < Capacity)
        {
            _rings[_count++] = new Ring { X = x, Y = y, From = from, To = to, Life = life, Age = 0, Color = color };
        }
    }

    public void Update(float dt)
    {
        for (int i = 0; i < _count; i++)
        {
            _rings[i].Age += dt;
            if (_rings[i].Age >= _rings[i].Life)
            {
                _rings[i--] = _rings[--_count];
            }
        }
    }

    public void Draw(InstanceLayer layer)
    {
        for (int i = 0; i < _count; i++)
        {
            Ring r = _rings[i];
            float t = r.Age / r.Life;
            float eased = 1f - (1f - t) * (1f - t);
            Color c = r.Color;
            c.A *= 1f - t;
            layer.Add(r.X, r.Y, 2f * (r.From + (r.To - r.From) * eased), c);
        }
    }

    private struct Ring
    {
        public float X;
        public float Y;
        public float From;
        public float To;
        public float Life;
        public float Age;
        public Color Color;
    }
}
