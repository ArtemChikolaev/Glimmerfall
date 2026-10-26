using System;
using System.Collections.Generic;
using Glimmerfall.Sim;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Декор арены — косметика, в сим и сид не входит, проходим насквозь. Реквизит из того же конвейера, что фигуры
/// (свет, контур и палитра общие): в центре — святилище со статуей и жаровнями (старт забега), вокруг — зоны
/// разного вида: погосты, руины с саркофагами, костяные поля с кольями, мёртвые рощи и места обрядов с кровавым
/// кругом на земле. Жаровни горят: пламя частицами и тёплый свет; круги обрядов тлеют красным. Места акта
/// (святилища и алтари сима) декор обходит — их рисует WorldView.
/// </summary>
public partial class MapDecor : Node2D
{
    private const float Margin = 96f;
    private const float PlaceClearance = 140f;

    private static readonly string[] Props = { "pillar", "statue", "tomb", "cross", "brazier", "sarcophagus", "bones", "stake", "deadtree" };
    private const int Pillar = 0;
    private const int Statue = 1;
    private const int Tomb = 2;
    private const int Cross = 3;
    private const int Brazier = 4;
    private const int Sarcophagus = 5;
    private const int Bones = 6;
    private const int Stake = 7;
    private const int DeadTree = 8;

    private readonly SpriteLayer[] _layers = new SpriteLayer[Props.Length];
    private readonly List<Placed> _placed = new();
    private readonly List<PointLight2D> _lights = new();
    private readonly List<Vector2> _keepClear = new();
    private Random _random = new();
    private Particles _particles = null!;
    private GroundDecals _decals = null!;
    private float _time;

    /// <param name="keepClear">Точки мест акта: декор не закрывает святилища и алтари.</param>
    public void Init(string suffix, ulong seed, Particles particles, Texture2D lightTexture, IEnumerable<Vector2> keepClear)
    {
        _particles = particles;
        _random = new Random(unchecked((int)seed ^ (int)(seed >> 32)));
        _keepClear.AddRange(keepClear);
        _decals = new GroundDecals();
        AddChild(_decals);
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
            AddLight(lightTexture, new Vector2(p.X, p.Y - 14), warm, 0.9f, 1.4f);
        }
        Color blood = MasterPalette.Instance["crimson3"];
        foreach (Vector2 circle in _decals.Circles)
        {
            AddLight(lightTexture, circle, blood, 0.7f, 1.6f);
        }
    }

    private void AddLight(Texture2D texture, Vector2 at, Color color, float energy, float scale)
    {
        var light = new PointLight2D { Texture = texture, Position = at, Color = color, Energy = energy, TextureScale = scale };
        AddChild(light);
        _lights.Add(light);
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
            Add(Pick(Tomb, Tomb, Cross, Pillar, Sarcophagus, Bones), at.X, at.Y);
        }
        for (int k = 0; k < 12; k++)
        {
            BoneField(Spot(c, 420));
        }
        for (int k = 0; k < 9; k++)
        {
            DeadGrove(Spot(c, 420));
        }
        for (int k = 0; k < 7; k++)
        {
            RitualSite(Spot(c, 520));
        }
        for (int k = 0; k < 70; k++)
        {
            Vector2 at = Spot(c, 260);
            _decals.Blood(at, 10 + Rnd() * 22, _random);
        }
    }

    /// <summary>Костяное поле: груды костей вразброс и колья с насаженными скелетами.</summary>
    private void BoneField(Vector2 at)
    {
        int piles = 6 + _random.Next(7);
        for (int k = 0; k < piles; k++)
        {
            Add(Bones, at.X + Jitter(150), at.Y + Jitter(110));
        }
        int stakes = 1 + _random.Next(3);
        for (int k = 0; k < stakes; k++)
        {
            Add(Stake, at.X + Jitter(110), at.Y + Jitter(80));
        }
        _decals.Blood(at + new Vector2(Jitter(60), Jitter(40)), 18 + Rnd() * 20, _random);
    }

    /// <summary>Мёртвая роща: кривые деревья тесной группой и кости у корней.</summary>
    private void DeadGrove(Vector2 at)
    {
        int trees = 4 + _random.Next(4);
        for (int k = 0; k < trees; k++)
        {
            Add(DeadTree, at.X + Jitter(170), at.Y + Jitter(120));
        }
        for (int k = 0; k < 3; k++)
        {
            Add(Bones, at.X + Jitter(140), at.Y + Jitter(100));
        }
    }

    /// <summary>Место обряда: кровавый круг с рунами, кольцо кольев и кости; круг тлеет красным светом.</summary>
    private void RitualSite(Vector2 at)
    {
        float radius = 64 + Rnd() * 20;
        _decals.Circle(at, radius);
        int stakes = 5 + _random.Next(3);
        float turn = Rnd() * MathF.Tau;
        for (int k = 0; k < stakes; k++)
        {
            float angle = turn + k * MathF.Tau / stakes;
            Add(Stake, at.X + MathF.Cos(angle) * (radius + 26), at.Y + MathF.Sin(angle) * (radius + 26) * 0.75f);
        }
        Add(Bones, at.X, at.Y);
        _decals.Blood(at + new Vector2(Jitter(20), Jitter(14)), 22, _random);
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
        foreach (Vector2 clear in _keepClear)
        {
            if (clear.DistanceSquaredTo(new Vector2(x, y)) < PlaceClearance * PlaceClearance)
            {
                return;
            }
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

/// <summary>
/// Пятна на земле — кровь и круги обрядов; рисуются один раз (Godot хранит команды рисования до QueueRedraw),
/// под реквизитом и фигурами, во тьме мира. Круги обрядов подсвечивает красный свет (<see cref="MapDecor"/>).
/// </summary>
public partial class GroundDecals : Node2D
{
    private readonly List<(Vector2 At, float Radius, float Squash, Color Color)> _pools = new();
    private readonly List<float> _circleRadii = new();

    public readonly List<Vector2> Circles = new();

    public void Blood(Vector2 at, float radius, Random random)
    {
        Color dark = MasterPalette.Instance["crimson1"];
        _pools.Add((at, radius, 0.55f + 0.2f * (float)random.NextDouble(), new Color(dark, 0.55f)));
        for (int k = 0; k < 3; k++)
        {
            var drop = new Vector2(((float)random.NextDouble() * 2 - 1) * radius * 1.6f, ((float)random.NextDouble() * 2 - 1) * radius);
            _pools.Add((at + drop, radius * (0.15f + 0.2f * (float)random.NextDouble()), 0.7f, new Color(dark, 0.5f)));
        }
    }

    public void Circle(Vector2 at, float radius)
    {
        Circles.Add(at);
        _circleRadii.Add(radius);
    }

    public override void _Draw()
    {
        foreach ((Vector2 at, float radius, float squash, Color color) in _pools)
        {
            DrawSetTransform(at, 0f, new Vector2(1f, squash));
            DrawCircle(Vector2.Zero, radius, color);
        }
        Color rune = MasterPalette.Instance["crimson2"];
        Color glow = MasterPalette.Instance["crimson3"];
        for (int i = 0; i < Circles.Count; i++)
        {
            float r = _circleRadii[i];
            DrawSetTransform(Circles[i], 0f, new Vector2(1f, 0.75f));
            DrawArc(Vector2.Zero, r, 0, Mathf.Tau, 48, new Color(glow, 0.7f), 2f);
            DrawArc(Vector2.Zero, r * 0.8f, 0, Mathf.Tau, 40, new Color(rune, 0.6f), 1.5f);
            for (int k = 0; k < 12; k++)
            {
                float a = k * Mathf.Tau / 12;
                var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                DrawLine(dir * r * 0.82f, dir * r * 0.98f, new Color(glow, 0.65f), 2f);
            }
            // пятиконечная звезда внутри круга
            for (int k = 0; k < 5; k++)
            {
                float a = -Mathf.Pi / 2 + k * Mathf.Tau * 2 / 5;
                float b = a + Mathf.Tau * 2 / 5;
                DrawLine(new Vector2(MathF.Cos(a), MathF.Sin(a)) * r * 0.78f, new Vector2(MathF.Cos(b), MathF.Sin(b)) * r * 0.78f,
                    new Color(rune, 0.55f), 1.5f);
            }
        }
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }
}
