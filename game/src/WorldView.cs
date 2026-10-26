using System.Collections.Generic;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Numerics;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Рисует мир сима: орда, снаряды, опыт и частицы — слоями MultiMesh, герой и свет фонаря — спрайтами, телеграфы —
/// линиями. Позиции интерполируются между тиками (alpha = доля тика), поэтому кадр плавный на любой частоте дисплея.
/// Ничего не пишет в сим: только читает массивы и события.
/// </summary>
public partial class WorldView : Node2D
{
    private const float FixToFloat = 1f / Fix.OneRaw;

    private World _world = null!;
    private InstanceLayer _gems = null!;
    private InstanceLayer _enemies = null!;
    private InstanceLayer _shots = null!;
    private InstanceLayer _projectiles = null!;
    private InstanceLayer _sparks = null!;
    private Sprite2D _hero = null!;
    private Sprite2D _lantern = null!;
    private TelegraphOverlay _telegraphs = null!;
    private readonly Particles _particles = new();

    public Vector2 HeroPosition { get; private set; }
    public int DrawnInstances { get; private set; }

    public void Init(World world)
    {
        _world = world;
        var additive = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        var ground = new Sprite2D
        {
            Texture = ProceduralArt.GroundTile(32),
            Centered = false,
            RegionEnabled = true,
            RegionRect = new Rect2(0, 0, SimConstants.ArenaSize, SimConstants.ArenaSize),
            TextureRepeat = TextureRepeatEnum.Enabled,
        };
        AddChild(ground);
        Texture2D body = ProceduralArt.Body(16);
        Texture2D glow = ProceduralArt.Glow(16, 1.6f);
        _gems = AddLayer(new InstanceLayer(glow, additive, 512));
        _enemies = AddLayer(new InstanceLayer(body, null, 1024));
        _shots = AddLayer(new InstanceLayer(glow, additive, 64));
        _projectiles = AddLayer(new InstanceLayer(glow, additive, 1024));
        _sparks = AddLayer(new InstanceLayer(glow, additive, 1024));
        _hero = new Sprite2D { Texture = ProceduralArt.Body(16) };
        AddChild(_hero);
        _lantern = new Sprite2D { Texture = ProceduralArt.Glow(128, 2.2f), Material = additive, Scale = new Vector2(2.6f, 2.6f), Modulate = new Color(1f, 0.7f, 0.35f, 0.32f) };
        AddChild(_lantern);
        _telegraphs = new TelegraphOverlay();
        AddChild(_telegraphs);
    }

    private InstanceLayer AddLayer(InstanceLayer layer)
    {
        AddChild(layer.Node);
        return layer;
    }

    /// <summary>События тика → частицы (вызывается после каждого шага сима).</summary>
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
                    break;
                case SimEventType.EnemyHit:
                    _particles.Burst(x, y, 2, new Color(1f, 0.85f, 0.6f), 90f, 0.18f, 3f);
                    break;
                case SimEventType.HeroHit:
                    _particles.Burst(x, y, 8, new Color(1f, 0.2f, 0.2f), 110f, 0.3f, 4f);
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
    }

    public void Render(float alpha, float frameSeconds)
    {
        World w = _world;
        _particles.Update(frameSeconds);

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

        _enemies.Begin();
        EnemyPool e = w.Enemies;
        for (int i = 0; i < e.HighWater; i++)
        {
            if (!e.Alive[i])
            {
                continue;
            }
            Color color = ProceduralArt.EnemyColors[e.Kind[i]];
            if (e.HitFlash[i] > 0)
            {
                color = color.Lerp(Colors.White, e.HitFlash[i] / 6f);
            }
            else if (e.State[i] == EnemyState.Telegraph)
            {
                color = color.Lerp(new Color(1f, 0.95f, 0.7f), 0.35f);
            }
            _enemies.Add(Lerp(e.PrevX[i], e.X[i], alpha), Lerp(e.PrevY[i], e.Y[i], alpha), e.Radius[i].Raw * FixToFloat * 2.2f, color);
        }
        _enemies.End();

        _shots.Begin();
        ShotPool s = w.Shots;
        for (int i = 0; i < s.HighWater; i++)
        {
            if (s.Alive[i])
            {
                _shots.Add(Lerp(s.PrevX[i], s.X[i], alpha), Lerp(s.PrevY[i], s.Y[i], alpha), 12f, new Color(0.55f, 1f, 0.35f));
            }
        }
        _shots.End();

        _projectiles.Begin();
        ProjectilePool p = w.Projectiles;
        for (int i = 0; i < p.HighWater; i++)
        {
            if (p.Alive[i])
            {
                _projectiles.Add(Lerp(p.PrevX[i], p.X[i], alpha), Lerp(p.PrevY[i], p.Y[i], alpha), p.Radius[i].Raw * FixToFloat * 4f, ProceduralArt.ProjectileColors[p.HeroKind[i]]);
            }
        }
        _projectiles.End();

        _sparks.Begin();
        _particles.Draw(_sparks);
        _sparks.End();

        Squad squad = w.Squad;
        HeroPosition = new Vector2(Lerp(squad.PrevX, squad.X, alpha), Lerp(squad.PrevY, squad.Y, alpha));
        _hero.Position = HeroPosition;
        _hero.Scale = new Vector2(1.25f, 1.25f);
        Color heroColor = ProceduralArt.HeroColors[squad.Kind[squad.Active]];
        heroColor.A = w.SquadInvulnerable && (Time.GetTicksMsec() / 60) % 2 == 0 ? 0.45f : 1f;
        _hero.Modulate = heroColor;
        _lantern.Position = HeroPosition;
        _telegraphs.Refresh(w, alpha);
        DrawnInstances = _gems.Count + _enemies.Count + _shots.Count + _projectiles.Count + _sparks.Count;
    }

    private static float Lerp(Fix from, Fix to, float alpha) => (from.Raw + (to.Raw - from.Raw) * alpha) * FixToFloat;
}

/// <summary>Телеграфы угроз: круг удара босса, линия тарана, прицел стрелка. Их немного — рисуются примитивами.</summary>
public partial class TelegraphOverlay : Node2D
{
    private World? _world;
    private float _alpha;

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
            var kind = _world.Config.EnemyKinds[e.Kind[i]];
            var at = new Vector2((e.PrevX[i].Raw + (e.X[i].Raw - e.PrevX[i].Raw) * _alpha) / Fix.OneRaw, (e.PrevY[i].Raw + (e.Y[i].Raw - e.PrevY[i].Raw) * _alpha) / Fix.OneRaw);
            var dir = new Vector2(e.DirX[i].Raw, e.DirY[i].Raw) / Fix.OneRaw;
            switch (kind.Behavior)
            {
                case Sim.Content.EnemyBehavior.Boss:
                    float radius = kind.SlamRadiusPx.Raw / (float)Fix.OneRaw;
                    float progress = 1f - (float)e.StateTicks[i] / kind.SlamWindupTicks;
                    DrawCircle(at, radius, new Color(1f, 0.6f, 0.2f, 0.12f));
                    DrawCircle(at, radius * progress, new Color(1f, 0.5f, 0.15f, 0.22f));
                    DrawArc(at, radius, 0, Mathf.Tau, 48, new Color(1f, 0.75f, 0.3f, 0.9f), 1.5f);
                    break;
                case Sim.Content.EnemyBehavior.Charger:
                    float reach = (kind.ChargeSpeedPxPerTick.Raw * kind.ChargeTicks) / (float)Fix.OneRaw;
                    Color line = e.State[i] == EnemyState.Charging ? new Color(1f, 0.3f, 0.2f, 0.9f) : new Color(1f, 0.85f, 0.4f, 0.55f);
                    DrawLine(at, at + dir * reach, line, 3f);
                    break;
                case Sim.Content.EnemyBehavior.Ranged:
                    DrawLine(at, at + dir * 140f, new Color(0.55f, 1f, 0.35f, 0.35f), 1f);
                    break;
            }
        }
    }
}
