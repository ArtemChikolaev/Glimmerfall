using System;
using System.Collections.Generic;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Numerics;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Рисует мир сима: герой и враги — анимированными листами конвейера (<see cref="SpriteLayer"/>), опыт, снаряды
/// и частицы — слоями MultiMesh, свет фонаря — спрайтом, телеграфы — линиями зарезервированного цвета.
/// Позиции интерполируются между тиками (alpha = доля тика). Ничего не пишет в сим: только читает массивы и события.
/// Порядок слоёв — порядок читаемости: трупы и рядовые, над ними элиты и боссы, над всеми — герой.
/// </summary>
public partial class WorldView : Node2D
{
    private const float FixToFloat = 1f / Fix.OneRaw;
    private const float CorpseFadeSeconds = 0.6f;
    private const float CullMargin = 96f;

    private World _world = null!;
    private InstanceLayer _gems = null!;
    private SpriteLayer _ghouls = null!;
    private SpriteLayer _brutes = null!;
    private SpriteLayer _heroLayer = null!;
    private InstanceLayer _shots = null!;
    private InstanceLayer _projectiles = null!;
    private InstanceLayer _sparks = null!;
    private Sprite2D _lantern = null!;
    private Sprite2D _ground = null!;
    private TelegraphOverlay _telegraphs = null!;
    private Particles _particles = null!;
    private readonly List<Corpse> _corpses = new();
    private readonly HeroAnimator _hero = new();
    private Color[] _projectileColors = Array.Empty<Color>();
    private Color _enemyShot;
    private float _time;

    public Vector2 HeroPosition { get; private set; }
    public int DrawnInstances { get; private set; }

    /// <summary>Живые враги в кадре 640×360 мировых единиц (для проверки читаемости «300+ врагов»).</summary>
    public int VisibleEnemies { get; private set; }

    /// <summary>Сетка 640×360 (gridScale 1) или 960×540 (1,5): листы с суффиксом _960 рисуются тем же размером в мире.</summary>
    public void Init(World world, float gridScale, int? particleSeed)
    {
        _world = world;
        _particles = new Particles(particleSeed);
        MasterPalette palette = MasterPalette.Instance;
        _projectileColors = new[] { palette["fire3"], palette["frost2"], palette["lightning2"] };
        _enemyShot = palette["telegraph2"];
        string suffix = gridScale > 1f ? $"_{Mathf.RoundToInt(640 * gridScale)}" : "";
        var additive = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        _ground = new Sprite2D
        {
            Texture = ProceduralArt.GroundTile(32),
            Centered = false,
            RegionEnabled = true,
            RegionRect = new Rect2(0, 0, SimConstants.ArenaSize, SimConstants.ArenaSize),
            TextureRepeat = TextureRepeatEnum.Enabled,
        };
        AddChild(_ground);
        // Пятно света фонаря — на полу, под фигурами: тёмный контур героя и врагов читается на освещённом камне,
        // а не высветляется вместе с ним (проверка серым скриншотом, G2.6).
        _lantern = new Sprite2D { Texture = ProceduralArt.Glow(128, 2.2f), Material = additive, Scale = new Vector2(2.6f, 2.6f), Modulate = new Color(palette["radiance1"], 0.30f) };
        AddChild(_lantern);
        Texture2D glow = ProceduralArt.Glow(16, 1.6f);
        _gems = AddLayer(new InstanceLayer(glow, additive, 512));
        _ghouls = AddSprites(new SpriteLayer(SpriteSheet.Load("ghoul" + suffix), 1024));
        _brutes = AddSprites(new SpriteLayer(SpriteSheet.Load("brute" + suffix), 64));
        _heroLayer = AddSprites(new SpriteLayer(SpriteSheet.Load("lamplighter" + suffix), 4));
        _hero.Sheet = _heroLayer.Sheet;
        _shots = AddLayer(new InstanceLayer(glow, additive, 64));
        _projectiles = AddLayer(new InstanceLayer(glow, additive, 1024));
        _sparks = AddLayer(new InstanceLayer(glow, additive, 1024));
        _telegraphs = new TelegraphOverlay();
        AddChild(_telegraphs);
    }

    private InstanceLayer AddLayer(InstanceLayer layer)
    {
        AddChild(layer.Node);
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
        foreach (CanvasItem item in new CanvasItem[] { _ground, _gems.Node, _lantern, _shots.Node, _projectiles.Node, _sparks.Node, _telegraphs })
        {
            item.Visible = !on;
        }
    }

    /// <summary>События тика → частицы, трупы и анимация героя (вызывается после каждого шага сима).</summary>
    public void Consume(List<SimEvent> events)
    {
        foreach (SimEvent e in events)
        {
            float x = e.X.Raw * FixToFloat;
            float y = e.Y.Raw * FixToFloat;
            switch (e.Type)
            {
                case SimEventType.EnemyKilled:
                    Color color = ProceduralArt.EnemyColors[e.Kind].Lightened(0.35f);
                    _particles.Burst(x, y, e.Kind >= 5 ? 40 : 6, color, e.Kind >= 5 ? 140f : 70f, 0.45f, 5f);
                    _corpses.Add(new Corpse { X = x, Y = y, Kind = e.Kind });
                    break;
                case SimEventType.EnemyHit:
                    _particles.Burst(x, y, 2, new Color(1f, 0.85f, 0.6f), 90f, 0.18f, 3f);
                    break;
                case SimEventType.HeroHit:
                    _particles.Burst(x, y, 8, new Color(1f, 0.2f, 0.2f), 110f, 0.3f, 4f);
                    _hero.Hit();
                    break;
                case SimEventType.Swap:
                    _particles.Burst(x, y, 36, ProceduralArt.HeroColors[e.Kind], 220f, 0.5f, 6f);
                    break;
                case SimEventType.Slam:
                    _particles.Burst(x, y, 48, new Color(1f, 0.8f, 0.3f), 260f, 0.4f, 6f);
                    break;
                case SimEventType.LevelUp:
                    _particles.Burst(x, y, 24, new Color(1f, 0.95f, 0.6f), 160f, 0.6f, 4f);
                    break;
            }
        }
        _hero.AfterTick(_world);
    }

    public void Render(float alpha, float frameSeconds)
    {
        World w = _world;
        _time += frameSeconds;
        _particles.Update(frameSeconds);
        Squad squad = w.Squad;
        HeroPosition = new Vector2(Lerp(squad.PrevX, squad.X, alpha), Lerp(squad.PrevY, squad.Y, alpha));
        Rect2 view = new Rect2(HeroPosition - new Vector2(320 + CullMargin, 180 + CullMargin), new Vector2(640 + 2 * CullMargin, 360 + 2 * CullMargin));

        _gems.Begin();
        GemPool g = w.Gems;
        for (int i = 0; i < g.HighWater; i++)
        {
            if (g.Alive[i])
            {
                _gems.Add(Lerp(g.PrevX[i], g.X[i], alpha), Lerp(g.PrevY[i], g.Y[i], alpha), 7f, new Color(0.45f, 0.9f, 1f, 0.9f));
            }
        }
        _gems.End();

        _ghouls.Begin();
        _brutes.Begin();
        DrawCorpses(frameSeconds, view);
        DrawEnemies(alpha, view);
        _ghouls.End();
        _brutes.End();

        _shots.Begin();
        ShotPool s = w.Shots;
        for (int i = 0; i < s.HighWater; i++)
        {
            if (s.Alive[i])
            {
                _shots.Add(Lerp(s.PrevX[i], s.X[i], alpha), Lerp(s.PrevY[i], s.Y[i], alpha), 12f, _enemyShot);
            }
        }
        _shots.End();

        _projectiles.Begin();
        ProjectilePool p = w.Projectiles;
        for (int i = 0; i < p.HighWater; i++)
        {
            if (p.Alive[i])
            {
                _projectiles.Add(Lerp(p.PrevX[i], p.X[i], alpha), Lerp(p.PrevY[i], p.Y[i], alpha), p.Radius[i].Raw * FixToFloat * 4f, _projectileColors[p.HeroKind[i]]);
            }
        }
        _projectiles.End();

        _sparks.Begin();
        _particles.Draw(_sparks);
        _sparks.End();

        _heroLayer.Begin();
        _hero.Draw(_heroLayer, HeroPosition, squad, w, frameSeconds);
        _heroLayer.End();
        _lantern.Position = HeroPosition;
        _telegraphs.Refresh(w, alpha);
        DrawnInstances = _gems.Count + _ghouls.Count + _brutes.Count + _heroLayer.Count + _shots.Count + _projectiles.Count + _sparks.Count;
    }

    private void DrawEnemies(float alpha, Rect2 view)
    {
        World w = _world;
        EnemyPool e = w.Enemies;
        int visible = 0;
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
            if (e.State[i] == EnemyState.Normal || !sheet.Has("attack"))
            {
                clip = sheet["run"];
                frame = clip.FrameAt(_time + (i * 0.618f % 1f) * clip.Duration);
            }
            else
            {
                clip = sheet["attack"];
                int windup = WindupTicks(kind);
                frame = e.State[i] == EnemyState.Telegraph && windup > 0
                    ? Math.Clamp((int)((1f - (float)e.StateTicks[i] / windup) * clip.WindupEnd), 0, clip.WindupEnd - 1)
                    : clip.WindupEnd + (int)(_time * clip.Fps) % Math.Max(clip.ActiveEnd - clip.WindupEnd, 1);
            }
            Color tint = Colors.White.Lerp(ProceduralArt.EnemyColors[e.Kind[i]], 0.28f);
            float flash = e.HitFlash[i] > 0 ? e.HitFlash[i] / 6f : 0f;
            int scale = kind.Behavior == EnemyBehavior.Boss ? 2 : 1;
            layer.Add(x, y, frame, clip.Row(direction), clip.Flip(direction), tint, flash, scale);
        }
        VisibleEnemies = visible;
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

    private struct Corpse
    {
        public float X;
        public float Y;
        public int Kind;
        public float Age;
    }
}

/// <summary>
/// Анимация героя в презентации: бег или стойка по движению, удар — когда у активного героя сбросился откат атаки,
/// вздрагивание — по событию попадания, смерть — когда отряд пал. Направление — по движению, в покое — по взгляду.
/// </summary>
public sealed class HeroAnimator
{
    private SpriteClip? _action;
    private float _actionTime;
    private float _actionRate = 1f;
    private int _lastCooldown;
    private int _lastActive = -1;
    private int _direction;
    private float _locomotionTime;

    public SpriteSheet Sheet { get; set; } = null!;

    public void Hit()
    {
        if (_action?.Name != "attack" && _action?.Name != "death")
        {
            Start("hit", 1f);
        }
    }

    /// <summary>После шага сима: новый выстрел активного героя запускает клип удара в темпе его отката.</summary>
    public void AfterTick(World world)
    {
        Squad s = world.Squad;
        int cooldown = s.AttackCooldown[s.Active];
        if (s.Active == _lastActive && cooldown > _lastCooldown && _action?.Name != "death")
        {
            SpriteClip attack = Sheet["attack"];
            Start("attack", Math.Max(1f, attack.Duration / Math.Max(cooldown / 60f, 0.05f)));
        }
        _lastCooldown = cooldown;
        _lastActive = s.Active;
        if ((s.AliveCount == 0 || world.Outcome == RunOutcome.Defeat) && _action?.Name != "death")
        {
            Start("death", 1f);
        }
    }

    public void Draw(SpriteLayer layer, Vector2 at, Squad squad, World world, float frameSeconds)
    {
        float dx = (squad.X.Raw - squad.PrevX.Raw) / (float)Fix.OneRaw;
        float dy = (squad.Y.Raw - squad.PrevY.Raw) / (float)Fix.OneRaw;
        bool moving = dx * dx + dy * dy > 0.01f;
        _direction = moving
            ? SpriteSheet.Direction(dx, dy)
            : SpriteSheet.Direction(squad.FacingX.Raw / (float)Fix.OneRaw, squad.FacingY.Raw / (float)Fix.OneRaw);
        _locomotionTime += frameSeconds;
        SpriteClip clip;
        int frame;
        if (_action != null)
        {
            _actionTime += frameSeconds * _actionRate;
            if (_action.Name != "death" && _actionTime >= _action.Duration)
            {
                _action = null;
            }
        }
        if (_action != null)
        {
            clip = _action;
            frame = clip.FrameAt(_actionTime);
        }
        else
        {
            clip = Sheet[moving ? "run" : "idle"];
            frame = clip.FrameAt(_locomotionTime);
        }
        Color tint = Colors.White.Lerp(ProceduralArt.HeroColors[squad.Kind[squad.Active]], 0.22f);
        // мигание неуязвимости (рывок, после удара) — по тикам сима: одинаково на любой частоте кадров и в снимках
        tint.A = world.SquadInvulnerable && (world.Tick / 4) % 2 == 0 ? 0.45f : 1f;
        layer.Add(at.X, at.Y, frame, clip.Row(_direction), clip.Flip(_direction), tint);
    }

    private void Start(string name, float rate)
    {
        _action = Sheet[name];
        _actionTime = 0f;
        _actionRate = rate;
    }
}

/// <summary>
/// Телеграфы угроз: круг удара босса, линия тарана, прицел стрелка — один зарезервированный цвет палитры,
/// которого нет у эффектов игрока (ADR 0003 п. 6). Их немного — рисуются примитивами.
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
