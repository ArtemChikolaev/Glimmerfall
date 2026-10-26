using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Частицы презентации (искры попаданий, пепел смертей). Живут только в game/: в сим и сид не входят,
/// поэтому случайность здесь своя и не детерминированная — это косметика.
/// </summary>
public sealed class Particles
{
    private const int Capacity = 6000;

    private readonly float[] _x = new float[Capacity];
    private readonly float[] _y = new float[Capacity];
    private readonly float[] _vx = new float[Capacity];
    private readonly float[] _vy = new float[Capacity];
    private readonly float[] _life = new float[Capacity];
    private readonly float[] _maxLife = new float[Capacity];
    private readonly float[] _size = new float[Capacity];
    private readonly Color[] _color = new Color[Capacity];
    private readonly Random _random;
    private int _count;

    /// <summary>seed — только для сравнимых скриншотов (G2.6); в игре разброс частиц свой на каждый запуск.</summary>
    public Particles(int? seed = null)
    {
        _random = seed is int value ? new Random(value) : new Random();
    }

    public int Count => _count;

    public void Burst(float x, float y, int n, Color color, float speed, float life, float size)
    {
        for (int k = 0; k < n && _count < Capacity; k++)
        {
            float angle = (float)(_random.NextDouble() * Math.Tau);
            float s = speed * (0.35f + 0.65f * (float)_random.NextDouble());
            int i = _count++;
            _x[i] = x;
            _y[i] = y;
            _vx[i] = MathF.Cos(angle) * s;
            _vy[i] = MathF.Sin(angle) * s;
            _maxLife[i] = life * (0.6f + 0.4f * (float)_random.NextDouble());
            _life[i] = _maxLife[i];
            _size[i] = size;
            _color[i] = color;
        }
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
                _color[i] = _color[_count];
                i--;
                continue;
            }
            _x[i] += _vx[i] * dt;
            _y[i] += _vy[i] * dt;
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
            c.A = t;
            layer.Add(_x[i], _y[i], _size[i] * (0.4f + 0.6f * t), c);
        }
    }
}
