using System;
using System.Collections.Generic;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Numerics;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Рисует мир сима. Слой 0 (под тьмой и светом): пол, декор, трупы, враги, элиты, герой, фонарь. Слой эффектов поверх:
/// опыт, снаряды по стихиям со шлейфами, искры попаданий по направлению удара, кольца, телеграфы. Отклик на удар —
/// вспышка, отброс фигуры на пару пикселей (только картинка), кадр вздрагивания, цифры урона (в интерфейсе).
/// Позиции интерполируются между тиками. Ничего не пишет в сим: только читает массивы и события.
/// </summary>
public partial class WorldView : Node2D
{
    private const float FixToFloat = 1f / Fix.OneRaw;
    private const float CorpseFadeSeconds = 0.6f;
    private const float CullMargin = 96f;
    private const int TrailProjectileLimit = 400;

    private World _world = null!;
    private WorldHost _host = null!;
    private Node2D _fx = null!;
    private Sprite2D _ground = null!;
    private MapDecor _decor = null!;
    private SpriteLayer _ghouls = null!;
    private SpriteLayer _brutes = null!;
    private SpriteLayer _heroLayer = null!;
    private PointLight2D _lantern = null!;
    private InstanceLayer _gems = null!;
    private InstanceLayer _flames = null!;
    private InstanceLayer _shots = null!;
    private InstanceLayer _projectiles = null!;
    private InstanceLayer _sparks = null!;
    private InstanceLayer _rings = null!;
    private TelegraphOverlay _telegraphs = null!;
    private Particles _particles = null!;
    private readonly Rings _ringFx = new();
    private readonly List<Corpse> _corpses = new();
    private readonly HeroAnimator _hero = new();
    private readonly float[] _kickX = new float[World.EnemyCapacity];
    private readonly float[] _kickY = new float[World.EnemyCapacity];
    private readonly int[] _kickHandle = new int[World.EnemyCapacity];
    private Element[] _elements = Array.Empty<Element>();
    private Color _enemyShot;
    private float _time;

    public Vector2 HeroPosition { get; private set; }
    public int DrawnInstances { get; private set; }

    /// <summary>Живые враги в кадре 640×360 мировых единиц (для проверки читаемости «300+ врагов»).</summary>
    public int VisibleEnemies { get; private set; }

    /// <summary>Сетка 640×360 или 960×540: листы с суффиксом _960 рисуются тем же размером в мире.</summary>
    public void Init(World world, WorldHost host, int? particleSeed)
    {
        _world = world;
        _host = host;
        _particles = new Particles(particleSeed);
        MasterPalette palette = MasterPalette.Instance;
        _elements = new[]
        {
            new Element(palette["fire2"], palette["fire4"], palette["fire3"], 15f, 6f),
            new Element(palette["frost2"], palette["frost4"], palette["frost3"], 13f, 5f),
            new Element(palette["lightning2"], palette["lightning4"], palette["lightning3"], 11f, 6f),
        };
        _enemyShot = palette["telegraph2"];
        string suffix = host.GridScale > 1f ? $"_{Mathf.RoundToInt(640 * host.GridScale)}" : "";
        var additive = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        Texture2D glow = ProceduralArt.Glow(16, 1.6f);
        Texture2D lightTexture = ProceduralArt.Glow(128, 1.3f);

        _ground = new Sprite2D
        {
            Texture = ProceduralArt.GroundTile(world.Seed),
            Centered = false,
            RegionEnabled = true,
            RegionRect = new Rect2(0, 0, SimConstants.ArenaSize, SimConstants.ArenaSize),
            TextureRepeat = TextureRepeatEnum.Enabled,
        };
        AddChild(_ground);
        _decor = new MapDecor();
        AddChild(_decor);
        _ghouls = AddSprites(new SpriteLayer(SpriteSheet.Load("ghoul" + suffix), 1024));
        _brutes = AddSprites(new SpriteLayer(SpriteSheet.Load("brute" + suffix), 64));
        _heroLayer = AddSprites(new SpriteLayer(SpriteSheet.Load("lamplighter" + suffix), 16));
        _hero.Sheet = _heroLayer.Sheet;
        _lantern = new PointLight2D { Texture = lightTexture, Color = palette["radiance2"], Energy = 1.05f, TextureScale = 3.6f };
        AddChild(_lantern);

        _fx = new Node2D { Name = "WorldFx" };
        host.Fx.AddChild(_fx);
        _gems = AddLayer(new InstanceLayer(glow, additive, 512));
        _flames = AddLayer(new InstanceLayer(glow, additive, 128));
        _rings = AddLayer(new InstanceLayer(ProceduralArt.Ring(64), additive, 64));
        _shots = AddLayer(new InstanceLayer(glow, additive, 64));
        _projectiles = AddLayer(new InstanceLayer(glow, additive, 1024));
        _sparks = AddLayer(new InstanceLayer(glow, additive, 1024));
        _telegraphs = new TelegraphOverlay();
        _fx.AddChild(_telegraphs);
        _decor.Init(suffix, world.Seed, _particles, lightTexture);
        host.Darkness.Color = new Color(0.56f, 0.53f, 0.64f);
        Array.Fill(_kickHandle, -1);
    }

    public override void _ExitTree()
    {
        _fx.QueueFree();
    }

    private InstanceLayer AddLayer(InstanceLayer layer)
    {
        _fx.AddChild(layer.Node);
        return layer;
    }

    private SpriteLayer AddSprites(SpriteLayer layer)
    {
        AddChild(layer.Node);
        return layer;
    }

    /// <summary>Маска для проверки читаемости: герой и враги плоскими цветами, всё прочее скрыто.</summary>
    public void SetMaskMode(bool on)
    {
        _heroLayer.SetMask(on, new Color(1, 0, 0));
        _ghouls.SetMask(on, new Color(0, 1, 0));
        _brutes.SetMask(on, new Color(0, 0, 1));
        _ground.Visible = !on;
        _decor.Visible = !on;
        _lantern.Visible = !on;
        _fx.Visible = !on;
    }

    /// <summary>События тика → отклик: искры, отброс фигур, трупы, кольца, анимация героя.</summary>
    public void Consume(List<SimEvent> events)
    {
        MasterPalette palette = MasterPalette.Instance;
        foreach (SimEvent e in events)
        {
            float x = e.X.Raw * FixToFloat;
            float y = e.Y.Raw * FixToFloat;
            float dx = e.DirX.Raw * FixToFloat;
            float dy = e.DirY.Raw * FixToFloat;
            switch (e.Type)
            {
                case SimEventType.EnemyHit:
                {
                    Element el = ElementOf(e.Source);
                    Kick(e.Target, dx, dy, e.Crit ? 4f : 2.5f);
                    _particles.Spray(x, y - 6, e.Crit ? 9 : 5, el.Spark, dx, dy, 0.6f, 120f, 0.22f, 3f);
                    if (e.Crit)
                    {
                        _ringFx.Add(x, y - 6, 3, 14, 0.22f, el.Core);
                    }
                    break;
                }
                case SimEventType.EnemyKilled:
                {
                    Element el = ElementOf(e.Source);
                    bool big = e.Kind >= EnemyIds.Ravager;
                    _corpses.Add(new Corpse { X = x, Y = y, Kind = e.Kind });
                    _particles.Spray(x, y - 8, big ? 30 : 10, el.Spark, dx, dy, 1.1f, big ? 170f : 110f, 0.4f, 4f);
                    _particles.Burst(x, y - 8, big ? 24 : 6, palette["stone3"], 40f, 0.9f, 3f, rise: 18f);
                    _ringFx.Add(x, y - 6, 4, big ? 70 : 20, big ? 0.5f : 0.25f, el.Core);
                    break;
                }
                case SimEventType.HeroHit:
                    _particles.Burst(x, y - 12, 10, palette["crimson4"], 110f, 0.3f, 4f);
                    _hero.Flash();
                    break;
                case SimEventType.HeroDown:
                    _particles.Burst(x, y - 12, 40, palette["crimson4"], 160f, 0.6f, 5f);
                    _ringFx.Add(x, y, 6, 90, 0.6f, palette["crimson4"]);
                    break;
                case SimEventType.Swap:
                    _particles.Burst(x, y - 12, 36, ProceduralArt.HeroColors[e.Kind], 220f, 0.5f, 6f);
                    _ringFx.Add(x, y - 10, 10, 80, 0.4f, ProceduralArt.HeroColors[e.Kind]);
                    break;
                case SimEventType.Dash:
                    _particles.Burst(x, y, 10, palette["stone4"], 60f, 0.35f, 4f);
                    _hero.Dash();
                    break;
                case SimEventType.LevelUp:
                    _ringFx.Add(x, y - 14, 8, 110, 0.7f, palette["radiance2"]);
                    _particles.Burst(x, y - 14, 30, palette["radiance2"], 90f, 1.0f, 4f, rise: 40f);
                    break;
                case SimEventType.Slam:
                    _ringFx.Add(x, y, 12, e.Amount, 0.45f, _enemyShot);
                    _particles.Burst(x, y, 50, palette["stone4"], 220f, 0.5f, 5f);
                    break;
                case SimEventType.HeroAttack:
                {
                    Element el = ElementOf(e.Kind);
                    _hero.Attack(dx, dy);
                    _particles.Spray(x + dx * 10, y - 14 + dy * 10, 6, el.Spark, dx, dy, 0.5f, 90f, 0.18f, 3f);
                    break;
                }
                case SimEventType.EnemyFired:
                    _particles.Burst(x, y - 8, 6, _enemyShot, 60f, 0.25f, 3f);
                    break;
            }
        }
    }

    public void Render(float alpha, float frameSeconds)
    {
        World w = _world;
        _time += frameSeconds;
        _particles.Update(frameSeconds);
        _ringFx.Update(frameSeconds);
        Squad squad = w.Squad;
        HeroPosition = new Vector2(Lerp(squad.PrevX, squad.X, alpha), Lerp(squad.PrevY, squad.Y, alpha));
        Vector2 half = _host.HalfView;
        var view = new Rect2(HeroPosition - half - new Vector2(CullMargin, CullMargin), 2 * half + new Vector2(2 * CullMargin, 2 * CullMargin));

        _flames.Begin();
        _decor.Render(HeroPosition, half, frameSeconds, _flames);
        _flames.End();

        _gems.Begin();
        GemPool g = w.Gems;
        Color gemColor = MasterPalette.Instance["frost3"];
        for (int i = 0; i < g.HighWater; i++)
        {
            if (g.Alive[i])
            {
                float pulse = 6f + 1.5f * MathF.Sin(_time * 6f + i);
                _gems.Add(Lerp(g.PrevX[i], g.X[i], alpha), Lerp(g.PrevY[i], g.Y[i], alpha), pulse, new Color(gemColor, 0.9f));
            }
        }
        _gems.End();

        _ghouls.Begin();
        _brutes.Begin();
        DrawCorpses(frameSeconds, view);
        DrawEnemies(alpha, view, frameSeconds);
        _ghouls.End();
        _brutes.End();

        _shots.Begin();
        ShotPool s = w.Shots;
        for (int i = 0; i < s.HighWater; i++)
        {
            if (s.Alive[i])
            {
                float x = Lerp(s.PrevX[i], s.X[i], alpha);
                float y = Lerp(s.PrevY[i], s.Y[i], alpha);
                _shots.AddOriented(x, y, s.VX[i].Raw, s.VY[i].Raw, 14f, 9f, new Color(_enemyShot, 0.95f));
                _shots.Add(x, y, 5f, new Color(1f, 0.9f, 0.95f, 0.9f));
            }
        }
        _shots.End();

        DrawProjectiles(alpha, frameSeconds);

        _rings.Begin();
        _ringFx.Draw(_rings);
        _rings.End();
        _sparks.Begin();
        _particles.Draw(_sparks);
        _sparks.End();

        _heroLayer.Begin();
        _hero.Draw(_heroLayer, HeroPosition, squad, w, frameSeconds);
        _heroLayer.End();
        _lantern.Position = HeroPosition + new Vector2(0, -12);
        _lantern.Energy = 1.0f + 0.06f * MathF.Sin(_time * 11f) * MathF.Sin(_time * 5.3f);
        _telegraphs.Refresh(w, alpha);
        DrawnInstances = _gems.Count + _ghouls.Count + _brutes.Count + _heroLayer.Count + _shots.Count + _projectiles.Count + _sparks.Count;
    }

    private void DrawProjectiles(float alpha, float frameSeconds)
    {
        _projectiles.Begin();
        ProjectilePool p = _world.Projectiles;
        bool trails = frameSeconds > 0 && p.Count <= TrailProjectileLimit;
        for (int i = 0; i < p.HighWater; i++)
        {
            if (!p.Alive[i])
            {
                continue;
            }
            Element el = ElementOf(p.HeroKind[i]);
            float x = Lerp(p.PrevX[i], p.X[i], alpha);
            float y = Lerp(p.PrevY[i], p.Y[i], alpha) - 10f;
            float vx = p.VX[i].Raw;
            float vy = p.VY[i].Raw;
            _projectiles.AddOriented(x, y, vx, vy, el.Length * 1.6f, el.Width * 1.8f, new Color(el.Glow, 0.55f));
            _projectiles.AddOriented(x, y, vx, vy, el.Length, el.Width * 0.7f, el.Core);
            if (trails && _particles.Random01() < 0.6f)
            {
                _particles.Trail(x, y, el.Spark, 0.25f, 3f);
            }
        }
        _projectiles.End();
    }

    private void DrawEnemies(float alpha, Rect2 view, float frameSeconds)
    {
        World w = _world;
        EnemyPool e = w.Enemies;
        float decay = MathF.Exp(-18f * frameSeconds);
        int visible = 0;
        Vector2 half = _host.HalfView;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            float x = Lerp(e.PrevX[i], e.X[i], alpha);
            float y = Lerp(e.PrevY[i], e.Y[i], alpha);
            if (!view.HasPoint(new Vector2(x, y)))
            {
                continue;
            }
            if (Math.Abs(x - HeroPosition.X) <= 320 && Math.Abs(y - HeroPosition.Y) <= 180)
            {
                visible++;
            }
            if (_kickHandle[i] == e.Handle(i))
            {
                x += _kickX[i];
                y += _kickY[i];
                _kickX[i] *= decay;
                _kickY[i] *= decay;
            }
            EnemyKind kind = w.Config.EnemyKinds[e.Kind[i]];
            SpriteLayer layer = LayerFor(kind);
            SpriteSheet sheet = layer.Sheet;
            float dx = (e.X[i].Raw - e.PrevX[i].Raw) * FixToFloat;
            float dy = (e.Y[i].Raw - e.PrevY[i].Raw) * FixToFloat;
            if (dx * dx + dy * dy < 0.0025f)
            {
                dx = (w.Squad.X.Raw - e.X[i].Raw) * FixToFloat;
                dy = (w.Squad.Y.Raw - e.Y[i].Raw) * FixToFloat;
            }
            int direction = SpriteSheet.Direction(dx, dy);
            SpriteClip clip;
            int frame;
            if (e.State[i] != EnemyState.Normal && sheet.Has("attack"))
            {
                clip = sheet["attack"];
                int windup = WindupTicks(kind);
                frame = e.State[i] == EnemyState.Telegraph && windup > 0
                    ? Math.Clamp((int)((1f - (float)e.StateTicks[i] / windup) * clip.WindupEnd), 0, clip.WindupEnd - 1)
                    : clip.WindupEnd + (int)(_time * clip.Fps) % Math.Max(clip.ActiveEnd - clip.WindupEnd, 1);
            }
            else if (e.HitFlash[i] > 0 && sheet.Has("hit"))
            {
                clip = sheet["hit"];
                frame = Math.Min(clip.Frames - 1, (6 - e.HitFlash[i]) * clip.Frames / 6);
            }
            else
            {
                clip = sheet["run"];
                frame = clip.FrameAt(_time + (i * 0.618f % 1f) * clip.Duration);
            }
            Color tint = Colors.White.Lerp(ProceduralArt.EnemyColors[e.Kind[i]], 0.28f);
            // крупных бьют непрерывно — полная вспышка прятала бы фигуру; им — слабее
            float flash = e.HitFlash[i] > 0 ? e.HitFlash[i] / 6f * (layer == _brutes ? 0.35f : 0.8f) : 0f;
            int scale = kind.Behavior == EnemyBehavior.Boss ? 2 : 1;
            layer.Add(x, y, frame, clip.Row(direction), clip.Flip(direction), tint, flash, scale);
        }
        VisibleEnemies = visible;
    }

    /// <summary>Отброс фигуры по направлению удара — только картинка, позиция в симе не меняется.</summary>
    private void Kick(int handle, float dx, float dy, float pixels)
    {
        if (handle < 0)
        {
            return;
        }
        int slot = EnemyPool.SlotOf(handle);
        float norm = MathF.Sqrt(dx * dx + dy * dy);
        if (norm < 1e-4f)
        {
            return;
        }
        if (_kickHandle[slot] != handle)
        {
            _kickHandle[slot] = handle;
            _kickX[slot] = 0;
            _kickY[slot] = 0;
        }
        _kickX[slot] = Math.Clamp(_kickX[slot] + dx / norm * pixels, -5f, 5f);
        _kickY[slot] = Math.Clamp(_kickY[slot] + dy / norm * pixels, -5f, 5f);
    }

    private void DrawCorpses(float frameSeconds, Rect2 view)
    {
        for (int k = _corpses.Count - 1; k >= 0; k--)
        {
            Corpse c = _corpses[k];
            c.Age += frameSeconds;
            EnemyKind kind = _world.Config.EnemyKinds[c.Kind];
            SpriteLayer layer = LayerFor(kind);
            SpriteClip death = layer.Sheet["death"];
            float fade = 1f - Math.Max(0f, c.Age - death.Duration) / CorpseFadeSeconds;
            if (fade <= 0f)
            {
                _corpses.RemoveAt(k);
                continue;
            }
            _corpses[k] = c;
            if (view.HasPoint(new Vector2(c.X, c.Y)))
            {
                Color tint = Colors.White.Lerp(ProceduralArt.EnemyColors[c.Kind], 0.28f) * new Color(1, 1, 1, fade);
                int scale = kind.Behavior == EnemyBehavior.Boss ? 2 : 1;
                layer.Add(c.X, c.Y, death.FrameAt(c.Age), death.Row(0), death.Flip(0), tint, 0f, scale, corpse: true);
            }
        }
    }

    private Element ElementOf(int heroKind) => heroKind >= 0 && heroKind < _elements.Length ? _elements[heroKind] : _elements[0];

    /// <summary>Лист элиты (60 px, ADR 0003: элиты 48–72) — только элитам и боссу; рядовые любого радиуса — гули.</summary>
    private SpriteLayer LayerFor(EnemyKind kind) => kind.Elite || kind.Behavior == EnemyBehavior.Boss ? _brutes : _ghouls;

    private static int WindupTicks(EnemyKind kind) => kind.Behavior switch
    {
        EnemyBehavior.Ranged => kind.AimTicks,
        EnemyBehavior.Charger => kind.ChargeWindupTicks,
        EnemyBehavior.Boss => kind.SlamWindupTicks,
        _ => 0,
    };

    private static float Lerp(Fix from, Fix to, float alpha) => (from.Raw + (to.Raw - from.Raw) * alpha) * FixToFloat;

    /// <summary>Стихия героя в эффектах: ореол, ядро и искры — рампы палитры (стиль-библия, §4).</summary>
    private readonly record struct Element(Color Glow, Color Core, Color Spark, float Length, float Width);

    private struct Corpse
    {
        public float X;
        public float Y;
        public int Kind;
        public float Age;
    }
}

/// <summary>
/// Анимация героя в презентации: бег или стойка по движению; удар — по событию атаки, лицом к цели; вздрагивание —
/// по попаданию; смерть — когда отряд пал; во время рывка — шлейф полупрозрачных копий.
/// </summary>
public sealed class HeroAnimator
{
    private const int GhostCount = 5;

    private readonly Ghost[] _ghosts = new Ghost[GhostCount];
    private SpriteClip? _action;
    private float _actionTime;
    private int _actionDirection = -1;
    private int _direction;
    private float _locomotionTime;
    private float _flash;
    private float _dashTime;
    private float _ghostTimer;
    private int _ghostNext;

    public SpriteSheet Sheet { get; set; } = null!;

    public void Flash()
    {
        _flash = 0.12f;
        if (_action?.Name is not ("attack" or "death"))
        {
            Start("hit", -1);
        }
    }

    public void Attack(float dx, float dy)
    {
        if (_action?.Name == "death")
        {
            return;
        }
        Start("attack", dx * dx + dy * dy > 1e-4f ? SpriteSheet.Direction(dx, dy) : -1);
    }

    public void Dash() => _dashTime = 0.2f;

    public void Draw(SpriteLayer layer, Vector2 at, Squad squad, World world, float frameSeconds)
    {
        if ((squad.AliveCount == 0 || world.Outcome == RunOutcome.Defeat) && _action?.Name != "death")
        {
            Start("death", -1);
        }
        float dx = (squad.X.Raw - squad.PrevX.Raw) / (float)Fix.OneRaw;
        float dy = (squad.Y.Raw - squad.PrevY.Raw) / (float)Fix.OneRaw;
        bool moving = dx * dx + dy * dy > 0.01f;
        _direction = moving
            ? SpriteSheet.Direction(dx, dy)
            : SpriteSheet.Direction(squad.FacingX.Raw / (float)Fix.OneRaw, squad.FacingY.Raw / (float)Fix.OneRaw);
        _locomotionTime += frameSeconds;
        _flash = Math.Max(0f, _flash - frameSeconds);
        if (_action != null)
        {
            _actionTime += frameSeconds;
            if (_action.Name != "death" && _actionTime >= _action.Duration)
            {
                _action = null;
            }
        }
        SpriteClip clip;
        int frame;
        int direction = _direction;
        if (_action != null)
        {
            clip = _action;
            frame = clip.FrameAt(_actionTime);
            if (_actionDirection >= 0)
            {
                direction = _actionDirection;
            }
        }
        else
        {
            clip = Sheet[moving ? "run" : "idle"];
            frame = clip.FrameAt(_locomotionTime);
        }
        Color tint = Colors.White.Lerp(ProceduralArt.HeroColors[squad.Kind[squad.Active]], 0.22f);
        tint.A = world.SquadInvulnerable && (world.Tick / 4) % 2 == 0 ? 0.55f : 1f;

        _dashTime = Math.Max(0f, _dashTime - frameSeconds);
        _ghostTimer += frameSeconds;
        if (_dashTime > 0f && _ghostTimer > 0.03f)
        {
            _ghostTimer = 0;
            _ghosts[_ghostNext] = new Ghost { X = at.X, Y = at.Y, Column = frame, Row = clip.Row(direction), Flip = clip.Flip(direction), Age = 0 };
            _ghostNext = (_ghostNext + 1) % GhostCount;
        }
        Color ghostTint = UiKit.WithAlpha(MasterPalette.Instance["radiance2"], 1f);
        for (int g = 0; g < GhostCount; g++)
        {
            ref Ghost ghost = ref _ghosts[g];
            if (ghost.Age >= 0.25f)
            {
                continue;
            }
            ghost.Age += frameSeconds;
            ghostTint.A = 0.45f * (1f - ghost.Age / 0.25f);
            layer.Add(ghost.X, ghost.Y, ghost.Column, ghost.Row, ghost.Flip, ghostTint, 0.6f);
        }
        layer.Add(at.X, at.Y, frame, clip.Row(direction), clip.Flip(direction), tint, _flash > 0 ? 0.8f : 0f);
    }

    private void Start(string name, int direction)
    {
        _action = Sheet[name];
        _actionTime = 0f;
        _actionDirection = direction;
    }

    private struct Ghost
    {
        public float X;
        public float Y;
        public int Column;
        public int Row;
        public bool Flip;
        public float Age;
    }

    public HeroAnimator()
    {
        for (int g = 0; g < GhostCount; g++)
        {
            _ghosts[g].Age = 1f;
        }
    }
}

/// <summary>
/// Телеграфы угроз: круг удара босса, линия тарана, прицел стрелка — один зарезервированный цвет палитры,
/// которого нет у эффектов игрока (ADR 0003 п. 6). Рисуются поверх всего и не темнеют во тьме.
/// </summary>
public partial class TelegraphOverlay : Node2D
{
    private World? _world;
    private float _alpha;
    private readonly Color _dim = MasterPalette.Instance["telegraph1"];
    private readonly Color _hot = MasterPalette.Instance["telegraph2"];

    public void Refresh(World world, float alpha)
    {
        _world = world;
        _alpha = alpha;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_world == null)
        {
            return;
        }
        EnemyPool e = _world.Enemies;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i] || e.State[i] == EnemyState.Normal)
            {
                continue;
            }
            EnemyKind kind = _world.Config.EnemyKinds[e.Kind[i]];
            var at = new Vector2((e.PrevX[i].Raw + (e.X[i].Raw - e.PrevX[i].Raw) * _alpha) / Fix.OneRaw, (e.PrevY[i].Raw + (e.Y[i].Raw - e.PrevY[i].Raw) * _alpha) / Fix.OneRaw);
            var dir = new Vector2(e.DirX[i].Raw, e.DirY[i].Raw) / Fix.OneRaw;
            switch (kind.Behavior)
            {
                case EnemyBehavior.Boss:
                    float radius = kind.SlamRadiusPx.Raw / (float)Fix.OneRaw;
                    float progress = 1f - (float)e.StateTicks[i] / kind.SlamWindupTicks;
                    DrawCircle(at, radius, new Color(_dim, 0.14f));
                    DrawCircle(at, radius * progress, new Color(_dim, 0.26f));
                    DrawArc(at, radius, 0, Mathf.Tau, 48, new Color(_hot, 0.9f), 1.5f);
                    break;
                case EnemyBehavior.Charger:
                    float reach = (kind.ChargeSpeedPxPerTick.Raw * kind.ChargeTicks) / (float)Fix.OneRaw;
                    Color line = e.State[i] == EnemyState.Charging ? new Color(_hot, 0.95f) : new Color(_dim, 0.6f);
                    DrawLine(at, at + dir * reach, line, 3f);
                    break;
                case EnemyBehavior.Ranged:
                    DrawLine(at, at + dir * 140f, new Color(_hot, 0.4f), 1f);
                    break;
            }
        }
    }
}
