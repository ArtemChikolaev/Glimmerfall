using System;
using System.Collections.Generic;
using Glimmerfall.Sim;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Декор арены — косметика, в сим и сид не входит, проходим насквозь. Реквизит из того же конвейера, что фигуры
/// (свет, контур и палитра общие): в центре — святилище со статуей и жаровнями (старт забега), вокруг — погосты,
/// руины с саркофагами, одиночные надгробия и пары жаровен. Жаровни горят: пламя частицами и тёплый свет.
/// </summary>
public partial class MapDecor : Node2D
{
    private const float Margin = 96f;

    private static readonly string[] Props = { "pillar", "statue", "tomb", "cross", "brazier", "sarcophagus" };
    private const int Pillar = 0;
    private const int Statue = 1;
    private const int Tomb = 2;
    private const int Cross = 3;
    private const int Brazier = 4;
    private const int Sarcophagus = 5;

    private readonly SpriteLayer[] _layers = new SpriteLayer[Props.Length];
    private readonly List<Placed> _placed = new();
    private readonly List<PointLight2D> _lights = new();
    private Random _random = new();
    private Particles _particles = null!;
    private float _time;

    public void Init(string suffix, ulong seed, Particles particles, Texture2D lightTexture)
    {
        _particles = particles;
        _random = new Random(unchecked((int)seed ^ (int)(seed >> 32)));
        for (int k = 0; k < Props.Length; k++)
        {
            _layers[k] = new SpriteLayer(SpriteSheet.Load(Props[k] + suffix), 64);
            AddChild(_layers[k].Node);
        }
        Layout();
        Color warm = MasterPalette.Instance["fire3"];
        foreach (Placed p in _placed)
        {
            if (p.Kind != Brazier)
            {
                continue;
            }
            var light = new PointLight2D
            {
                Texture = lightTexture,
                Position = new Vector2(p.X, p.Y - 14),
                Color = warm,
                Energy = 0.9f,
                TextureScale = 1.4f,
            };
            AddChild(light);
            _lights.Add(light);
        }
    }

    private void Layout()
    {
        float c = SimConstants.ArenaSize / 2f;
        // святилище в центре — отряд начинает акт у света
        Add(Statue, c, c - 70);
        Add(Brazier, c - 74, c - 46);
        Add(Brazier, c + 74, c - 46);
        Add(Brazier, c - 90, c + 56);
        Add(Brazier, c + 90, c + 56);
        Add(Pillar, c - 150, c - 30);
        Add(Pillar, c + 150, c - 30);
        for (int k = 0; k < 14; k++)
        {
            Graveyard(Spot(c, 400));
        }
        for (int k = 0; k < 14; k++)
        {
            Ruin(Spot(c, 360));
        }
        for (int k = 0; k < 26; k++)
        {
            Vector2 at = Spot(c, 260);
            float angle = Rnd() * MathF.Tau;
            var step = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 70f;
            Add(Brazier, at.X - step.X, at.Y - step.Y);
            Add(Brazier, at.X + step.X, at.Y + step.Y);
        }
        for (int k = 0; k < 160; k++)
        {
            Vector2 at = Spot(c, 220);
            Add(Pick(Tomb, Tomb, Cross, Pillar, Sarcophagus), at.X, at.Y);
        }
    }

    private void Graveyard(Vector2 at)
    {
        int columns = 3 + _random.Next(3);
        int rows = 2 + _random.Next(3);
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (Rnd() < 0.15f)
                {
                    continue;
                }
                Add(Rnd() < 0.6f ? Tomb : Cross, at.X + (column - columns / 2f) * 36 + Jitter(5), at.Y + (row - rows / 2f) * 30 + Jitter(4));
            }
        }
        if (Rnd() < 0.5f)
        {
            Add(Statue, at.X, at.Y - rows * 15 - 40);
        }
    }

    private void Ruin(Vector2 at)
    {
        int pillars = 4 + _random.Next(5);
        float radius = 60 + Rnd() * 40;
        float turn = Rnd() * MathF.Tau;
        for (int k = 0; k < pillars; k++)
        {
            float angle = turn + k * MathF.Tau / pillars;
            if (Rnd() < 0.2f)
            {
                continue;
            }
            Add(Pillar, at.X + MathF.Cos(angle) * radius, at.Y + MathF.Sin(angle) * radius * 0.7f);
        }
        Add(Sarcophagus, at.X, at.Y);
        if (Rnd() < 0.6f)
        {
            Add(Brazier, at.X - 34, at.Y + 22);
            Add(Brazier, at.X + 34, at.Y + 22);
        }
    }

    /// <summary>Случайная точка арены вне круга старта.</summary>
    private Vector2 Spot(float center, float keepOut)
    {
        while (true)
        {
            float x = Margin + Rnd() * (SimConstants.ArenaSize - 2 * Margin);
            float y = Margin + Rnd() * (SimConstants.ArenaSize - 2 * Margin);
            if ((x - center) * (x - center) + (y - center) * (y - center) > keepOut * keepOut)
            {
                return new Vector2(x, y);
            }
        }
    }

    private void Add(int kind, float x, float y)
    {
        if (x < Margin || y < Margin || x > SimConstants.ArenaSize - Margin || y > SimConstants.ArenaSize - Margin)
        {
            return;
        }
        _placed.Add(new Placed { Kind = kind, X = x, Y = y, Variant = _random.Next(4), Phase = Rnd() * 10f });
    }

    private int Pick(params int[] kinds) => kinds[_random.Next(kinds.Length)];

    private float Rnd() => (float)_random.NextDouble();

    private float Jitter(float amount) => (Rnd() * 2 - 1) * amount;

    /// <summary>Видимый реквизит в слои, пламя жаровен — частицами в слой эффектов, свет мерцает.</summary>
    public void Render(Vector2 center, Vector2 halfView, float seconds, InstanceLayer flames)
    {
        _time += seconds;
        foreach (SpriteLayer layer in _layers)
        {
            layer.Begin();
        }
        float reachX = halfView.X + 80;
        float reachY = halfView.Y + 100;
        Color core = MasterPalette.Instance["fire4"];
        Color ember = MasterPalette.Instance["fire3"];
        foreach (Placed p in _placed)
        {
            if (Math.Abs(p.X - center.X) > reachX || Math.Abs(p.Y - center.Y) > reachY)
            {
                continue;
            }
            SpriteLayer layer = _layers[p.Kind];
            SpriteClip idle = layer.Sheet["idle"];
            layer.Add(p.X, p.Y, 0, idle.Row(p.Variant), false, Colors.White);
            if (p.Kind != Brazier)
            {
                continue;
            }
            float flicker = 0.75f + 0.25f * MathF.Sin(_time * 13f + p.Phase) * MathF.Sin(_time * 7.3f + p.Phase * 2f);
            float fy = p.Y - 15;
            flames.Add(p.X, fy - 3, 16f * flicker, new Color(ember, 0.55f));
            flames.Add(p.X, fy - 1, 8f * flicker, new Color(core, 0.9f));
            if (seconds > 0 && _particles.Random01() < seconds * 30f)
            {
                _particles.Trail(p.X, fy - 2, _particles.Random01() < 0.5f ? ember : core, 0.6f, 3f, 6f, rise: 26f);
            }
        }
        foreach (SpriteLayer layer in _layers)
        {
            layer.End();
        }
        for (int i = 0; i < _lights.Count; i++)
        {
            PointLight2D light = _lights[i];
            light.Visible = Math.Abs(light.Position.X - center.X) < reachX + 200 && Math.Abs(light.Position.Y - center.Y) < reachY + 200;
            light.Energy = 0.75f + 0.2f * MathF.Sin(_time * 9f + i * 1.7f) * MathF.Sin(_time * 4.1f + i);
        }
    }

    private struct Placed
    {
        public int Kind;
        public float X;
        public float Y;
        public int Variant;
        public float Phase;
    }
}
