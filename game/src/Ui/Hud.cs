using System;
using System.Collections.Generic;
using System.Globalization;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Numerics;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// HUD забега в родном разрешении: отряд с портретами и здоровьем, акт и цель, полоса босса, опыт и уровень,
/// рывок и смена героя с откатом, счёт поверженных. Читает мир каждый кадр, в сим ничего не пишет.
/// </summary>
public partial class Hud : Control
{
    private readonly HeroRow[] _heroes = new HeroRow[Squad.Size];
    private Label _act = null!;
    private Label _objective = null!;
    private Control _bossBox = null!;
    private ProgressBar _bossBar = null!;
    private Label _bossName = null!;
    private ProgressBar _xp = null!;
    private Label _level = null!;
    private Label _kills = null!;
    private Label _fps = null!;
    private Ability _dash = null!;
    private Ability _swap = null!;
    private Ability _assist = null!;
    private ColorRect _hurt = null!;
    private float _hurtPulse;

    public bool ShowFps { get; set; }

    public void Build(SpriteSheet[] heroSheets)
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        _hurt = new ColorRect { MouseFilter = MouseFilterEnum.Ignore, Material = HurtMaterial(), Color = Colors.White };
        _hurt.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_hurt);

        var squad = new VBoxContainer { Position = new Vector2(14, 12), MouseFilter = MouseFilterEnum.Ignore };
        squad.AddThemeConstantOverride("separation", 6);
        for (int i = 0; i < Squad.Size; i++)
        {
            _heroes[i] = new HeroRow(heroSheets);
            squad.AddChild(_heroes[i].Root);
        }
        AddChild(squad);

        var top = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.Begin };
        top.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterTop);
        top.GrowHorizontal = GrowDirection.Both;
        top.Position = new Vector2(0, 10);
        top.AddThemeConstantOverride("separation", 2);
        _act = UiKit.Text("", 24, UiKit.Wax, serif: true);
        _act.HorizontalAlignment = HorizontalAlignment.Center;
        top.AddChild(_act);
        _objective = UiKit.Text("", 18, UiKit.Glow);
        _objective.HorizontalAlignment = HorizontalAlignment.Center;
        top.AddChild(_objective);
        _bossBox = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        _bossName = UiKit.Text("Страж мрака", 18, UiKit.Wax, serif: true);
        _bossName.HorizontalAlignment = HorizontalAlignment.Center;
        _bossBox.AddChild(_bossName);
        _bossBar = Bar(MasterPalette.Instance["telegraph1"], new Vector2(520, 16));
        _bossBox.AddChild(_bossBar);
        top.AddChild(_bossBox);
        AddChild(top);

        _kills = UiKit.Text("", 20, UiKit.Wax);
        _kills.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
        _kills.GrowHorizontal = GrowDirection.Begin;
        _kills.Position += new Vector2(-16, 12);
        _kills.HorizontalAlignment = HorizontalAlignment.Right;
        AddChild(_kills);
        _fps = UiKit.Text("", 14, UiKit.WaxDim);
        _fps.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
        _fps.GrowHorizontal = GrowDirection.Begin;
        _fps.Position += new Vector2(-16, 40);
        _fps.HorizontalAlignment = HorizontalAlignment.Right;
        AddChild(_fps);

        var bottom = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        bottom.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomWide);
        bottom.GrowVertical = GrowDirection.Begin;
        bottom.OffsetLeft = 14;
        bottom.OffsetRight = -14;
        bottom.OffsetTop = -40;
        bottom.OffsetBottom = -12;
        bottom.AddThemeConstantOverride("separation", 10);
        _dash = new Ability("Рывок", "Пробел");
        bottom.AddChild(_dash.Root);
        _swap = new Ability("Смена", "Q");
        bottom.AddChild(_swap.Root);
        _assist = new Ability("Ассист", "E");
        _assist.Root.CustomMinimumSize = new Vector2(230, 0);
        bottom.AddChild(_assist.Root);
        _level = UiKit.Text("", 20, UiKit.Glow, serif: true);
        _level.CustomMinimumSize = new Vector2(70, 0);
        _level.HorizontalAlignment = HorizontalAlignment.Right;
        bottom.AddChild(_level);
        _xp = Bar(MasterPalette.Instance["frost2"], new Vector2(0, 12));
        _xp.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _xp.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        bottom.AddChild(_xp);
        AddChild(bottom);
    }

    /// <summary>Вспышка красной виньетки по краям экрана при уроне герою.</summary>
    public void Hurt(float strength) => _hurtPulse = Math.Max(_hurtPulse, strength);

    public void Refresh(World world, float frameSeconds)
    {
        Squad s = world.Squad;
        for (int i = 0; i < Squad.Size; i++)
        {
            _heroes[i].Refresh(world, i);
        }
        _act.Text = string.Format(CultureInfo.InvariantCulture, "Акт {0}/{1} · {2} · {3}",
            world.ActIndex + 1, world.Config.Acts, GameText.ActName(world.ActIndex), GameText.Clock(world.ActTick));
        _objective.Text = GameText.Objective(world);
        bool boss = world.IsBossAlive;
        _bossBox.Visible = boss;
        if (boss)
        {
            int slot = EnemyPool.SlotOf(world.BossHandle);
            _bossBar.MaxValue = world.Enemies.MaxHp[slot];
            _bossBar.Value = world.Enemies.Hp[slot];
        }
        _xp.MaxValue = Rules.XpToNext(world.Level);
        _xp.Value = world.Xp;
        _level.Text = $"Ур. {world.Level}";
        _kills.Text = $"Повержено: {world.Stats.TotalKills}";
        _fps.Visible = ShowFps;
        _fps.Text = $"{Engine.GetFramesPerSecond():0} кадр/с";
        _dash.Refresh(s.DashCooldown, Rules.DashCooldownTicks, s.DashTicks > 0);
        int helper = world.NextAliveReserve();
        _swap.Refresh(s.SwapCooldown, Rules.SwapCooldownTicks, false, helper < 0);
        _assist.Name = helper >= 0 ? $"Ассист: {GameText.HeroNames[s.Kind[helper]]}" : "Ассист";
        _assist.Refresh(s.AssistCooldown, Rules.AssistCooldownTicks, false, helper < 0);

        float lowHp = s.Alive[s.Active] ? 1f - (float)s.Hp[s.Active] / s.MaxHp[s.Active] : 0f;
        float danger = lowHp > 0.7f ? (lowHp - 0.7f) / 0.3f * (0.35f + 0.15f * MathF.Sin((float)Time.GetTicksMsec() / 180f)) : 0f;
        _hurtPulse = Math.Max(0f, _hurtPulse - frameSeconds * 2.5f);
        ((ShaderMaterial)_hurt.Material).SetShaderParameter("strength", Math.Clamp(Math.Max(_hurtPulse, danger), 0f, 1f));
    }

    private static ProgressBar Bar(Color fill, Vector2 size)
    {
        var bar = new ProgressBar { ShowPercentage = false, CustomMinimumSize = size, MouseFilter = MouseFilterEnum.Ignore };
        bar.AddThemeStyleboxOverride("background", UiKit.Box(UiKit.WithAlpha(MasterPalette.Instance["ink"], 0.85f), UiKit.BrassDark, 1, 0));
        var box = new StyleBoxFlat { BgColor = fill };
        box.SetCornerRadiusAll(3);
        bar.AddThemeStyleboxOverride("fill", box);
        return bar;
    }

    private static ShaderMaterial HurtMaterial()
    {
        var shader = new Shader
        {
            Code = @"
shader_type canvas_item;
uniform float strength = 0.0;
uniform vec4 tint : source_color = vec4(0.75, 0.08, 0.12, 1.0);
void fragment() {
    vec2 p = UV - 0.5;
    float edge = smoothstep(0.25, 0.75, length(p * vec2(1.0, 1.25)) * 1.2);
    COLOR = vec4(tint.rgb, edge * strength * 0.85);
}",
        };
        var material = new ShaderMaterial { Shader = shader };
        material.SetShaderParameter("tint", MasterPalette.Instance["crimson3"]);
        return material;
    }

    /// <summary>Строка героя: портрет из листа (тонирован цветом героя), имя, здоровье.</summary>
    private sealed class HeroRow
    {
        public readonly PanelContainer Root;
        private readonly TextureRect _portrait;
        private readonly Label _name;
        private readonly ProgressBar _hp;
        private readonly Label _hpText;
        private readonly StyleBoxFlat _active;
        private readonly StyleBoxFlat _idle;

        private readonly Texture2D[] _portraits;

        public HeroRow(SpriteSheet[] sheets)
        {
            _active = UiKit.Box(UiKit.SootLight, UiKit.Glow, 2, 4);
            _idle = UiKit.Box(UiKit.WithAlpha(MasterPalette.Instance["gloom1"], 0.7f), UiKit.WithAlpha(UiKit.BrassDark, 0.6f), 1, 4);
            Root = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore, CustomMinimumSize = new Vector2(300, 0) };
            var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            row.AddThemeConstantOverride("separation", 8);
            // портрет — голова и плечи из кадра стойки лицом к камере, по листу каждого героя
            _portraits = new Texture2D[sheets.Length];
            for (int k = 0; k < sheets.Length; k++)
            {
                SpriteSheet sheet = sheets[k];
                float size = 36 * sheet.GridScale;
                var region = new Rect2(sheet.AnchorX - size / 2, sheet["idle"].Row(0) * sheet.CellHeight + sheet.AnchorY - 54 * sheet.GridScale, size, size * 0.9f);
                _portraits[k] = new AtlasTexture { Atlas = sheet.Texture, Region = region };
            }
            _portrait = new TextureRect
            {
                Texture = _portraits[0],
                CustomMinimumSize = new Vector2(52, 47),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                TextureFilter = TextureFilterEnum.Nearest,
                MouseFilter = MouseFilterEnum.Ignore,
            };
            row.AddChild(_portrait);
            var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            column.AddThemeConstantOverride("separation", 2);
            var line = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            _name = UiKit.Text("", 17, UiKit.Wax, serif: true);
            _name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            line.AddChild(_name);
            _hpText = UiKit.Text("", 15, UiKit.WaxDim);
            line.AddChild(_hpText);
            column.AddChild(line);
            _hp = Bar(MasterPalette.Instance["crimson3"], new Vector2(200, 10));
            column.AddChild(_hp);
            row.AddChild(column);
            Root.AddChild(row);
        }

        public void Refresh(World world, int index)
        {
            Squad s = world.Squad;
            int kind = s.Kind[index];
            bool active = index == s.Active;
            bool alive = s.Alive[index];
            Root.AddThemeStyleboxOverride("panel", active ? _active : _idle);
            _name.Text = $"{GameText.HeroNames[kind]} · {GameText.HeroElements[kind]}";
            _hp.MaxValue = s.MaxHp[index];
            _hp.Value = s.Hp[index];
            _hpText.Text = alive ? $"{s.Hp[index]}/{s.MaxHp[index]}" : "пал";
            if (_portrait.Texture != _portraits[kind])
            {
                _portrait.Texture = _portraits[kind];
            }
            _portrait.Modulate = alive ? Colors.White : new Color(0.35f, 0.35f, 0.4f);
            Root.Modulate = active ? Colors.White : new Color(1, 1, 1, alive ? 0.8f : 0.5f);
        }
    }

    /// <summary>Значок способности с откатом: название, клавиша и заполнение готовности.</summary>
    private sealed class Ability
    {
        public readonly PanelContainer Root;
        private readonly Label _label;
        private readonly ProgressBar _ready;
        private readonly string _key;

        public Ability(string name, string key)
        {
            Name = name;
            _key = key;
            Root = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore, CustomMinimumSize = new Vector2(150, 0) };
            var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            column.AddThemeConstantOverride("separation", 2);
            _label = UiKit.Text("", 15, UiKit.Wax);
            column.AddChild(_label);
            _ready = Bar(MasterPalette.Instance["radiance1"], new Vector2(120, 5));
            column.AddChild(_ready);
            Root.AddChild(column);
        }

        /// <summary>Подпись способности; у ассиста — с именем героя, который выпрыгнет.</summary>
        public string Name { get; set; }

        public void Refresh(int cooldown, int full, bool active, bool unavailable = false)
        {
            _ready.MaxValue = full;
            _ready.Value = full - cooldown;
            string state = unavailable ? "нет резерва" : cooldown > 0 ? $"{(cooldown + 59) / 60} с" : "готово";
            _label.Text = $"{Name} [{_key}] · {state}";
            Root.Modulate = active ? new Color(1.3f, 1.2f, 0.9f) : cooldown > 0 || unavailable ? new Color(1, 1, 1, 0.65f) : Colors.White;
        }
    }
}

/// <summary>
/// Выбор благословения или награды чекпоинта — карточками: название, уровень, что даст (числа из правил сима).
/// Мышь, клавиши 1–3, стрелки и геймпад. Сим в это время стоит на модалке.
/// </summary>
public partial class ChoiceCards : Control
{
    private readonly List<Button> _cards = new();
    private Label _header = null!;
    private HBoxContainer _row = null!;
    private Sfx _sfx = null!;
    private int _shownStep = -1;

    public event Action<int>? Chosen;

    public void Build(Sfx sfx)
    {
        _sfx = sfx;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(UiKit.Scrim(0.45f));
        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        column.AddThemeConstantOverride("separation", 18);
        _header = UiKit.Text("", 32, UiKit.Glow, serif: true);
        _header.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(_header);
        _row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        _row.AddThemeConstantOverride("separation", 18);
        column.AddChild(_row);
        var hint = UiKit.Text("Клик, клавиши 1–3 или стрелки и Enter", 15, UiKit.WaxDim);
        hint.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(hint);
        AddChild(UiKit.Centered(column));
    }

    /// <summary>Показывает модалку текущего шага сима (раз на шаг — фокус не прыгает каждый кадр).</summary>
    public void Show(World world)
    {
        if (_shownStep == world.StepCount && Visible)
        {
            return;
        }
        _shownStep = world.StepCount;
        foreach (Button card in _cards)
        {
            card.QueueFree();
        }
        _cards.Clear();
        bool blessing = world.Modal == ModalKind.Blessing;
        _header.Text = blessing ? $"Уровень {world.Level - world.PendingLevelUps}! Выбери благословение" : "Акт пройден — привал. Выбери награду";
        for (int k = 0; k < world.ModalOptionCount; k++)
        {
            int option = world.ModalOption(k);
            string title;
            string body;
            string level = "";
            if (blessing)
            {
                var b = (Blessing)option;
                title = GameText.BlessingTitle(b);
                body = GameText.BlessingDescription(b, world);
                int current = world.BlessingLevels[option];
                int max = Blessings.MaxLevel(b);
                level = max < 20 ? new string('●', current) + new string('○', max - current) : "";
            }
            else
            {
                var c = (CheckpointOption)option;
                title = GameText.CheckpointTitle(c);
                body = GameText.CheckpointDescription(c);
            }
            _cards.Add(Card(k, title, level, body));
        }
        Visible = true;
        _cards[0].GrabFocus();
        _sfx.Play(Sound.UiHover, 0.8f, 0f);
    }

    private Button Card(int index, string title, string level, string body)
    {
        var card = new Button { CustomMinimumSize = new Vector2(300, 250), FocusMode = FocusModeEnum.All };
        card.AddThemeStyleboxOverride("normal", UiKit.Box(UiKit.Soot, UiKit.BrassDark, 2, 12));
        card.AddThemeStyleboxOverride("hover", UiKit.Box(UiKit.SootLight, UiKit.Brass, 3, 12));
        card.AddThemeStyleboxOverride("focus", UiKit.Box(new Color(0, 0, 0, 0), UiKit.Glow, 3, 12));
        card.AddThemeStyleboxOverride("pressed", UiKit.Box(UiKit.SootLight, UiKit.Glow, 3, 12));
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        column.OffsetLeft = 18;
        column.OffsetRight = -18;
        column.OffsetTop = 16;
        column.OffsetBottom = -14;
        column.AddThemeConstantOverride("separation", 8);
        column.AddChild(UiKit.Text(title, 28, UiKit.Glow, serif: true));
        if (level.Length > 0)
        {
            column.AddChild(UiKit.Text(level, 16, UiKit.Brass));
        }
        var text = UiKit.Text(body, 17, UiKit.Wax);
        text.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        text.SizeFlagsVertical = SizeFlags.ExpandFill;
        column.AddChild(text);
        var key = UiKit.Text($"[{index + 1}]", 18, UiKit.WaxDim);
        key.HorizontalAlignment = HorizontalAlignment.Right;
        column.AddChild(key);
        card.AddChild(column);
        card.Pressed += () => Choose(index);
        card.MouseEntered += () => card.GrabFocus();
        card.FocusEntered += () => _sfx.Play(Sound.UiHover, 0.5f, 0f);
        _row.AddChild(card);
        return card;
    }

    public void Choose(int index)
    {
        if (!Visible || index < 0 || index >= _cards.Count)
        {
            return;
        }
        _sfx.Play(Sound.UiClick, 0.9f, 0f);
        Visible = false;
        Chosen?.Invoke(index);
    }
}

/// <summary>Крупная надпись по центру с появлением и угасанием: начало акта, босс, уровень.</summary>
public partial class Banner : Control
{
    private Label _title = null!;
    private Label _subtitle = null!;
    private float _age;
    private float _duration;

    public void Build()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        column.GrowHorizontal = GrowDirection.Both;
        column.GrowVertical = GrowDirection.Both;
        column.Position += new Vector2(0, -120);
        _title = UiKit.Text("", 54, UiKit.Glow, serif: true);
        _title.HorizontalAlignment = HorizontalAlignment.Center;
        _title.AddThemeConstantOverride("outline_size", 10);
        column.AddChild(_title);
        _subtitle = UiKit.Text("", 22, UiKit.Wax, serif: true);
        _subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(_subtitle);
        AddChild(column);
        Visible = false;
    }

    public void Show(string title, string subtitle, float seconds = 3f, Color? color = null)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        _title.AddThemeColorOverride("font_color", color ?? UiKit.Glow);
        _age = 0;
        _duration = seconds;
        Visible = true;
    }

    public override void _Process(double delta)
    {
        if (!Visible)
        {
            return;
        }
        _age += (float)delta;
        float fadeIn = Math.Min(1f, _age / 0.35f);
        float fadeOut = Math.Min(1f, (_duration - _age) / 0.6f);
        Modulate = new Color(1, 1, 1, Math.Clamp(Math.Min(fadeIn, fadeOut), 0f, 1f));
        if (_age >= _duration)
        {
            Visible = false;
        }
    }
}

/// <summary>
/// Цифры урона в родном разрешении над точкой мира: мелкие, всплывают и гаснут, крит крупнее и теплее; не больше
/// <see cref="Capacity"/> одновременно (ADR 0003 п. 6 — лимит плотности).
/// </summary>
public partial class DamageNumbers : Control
{
    private const int Capacity = 48;
    private const float Life = 0.7f;

    private readonly List<Entry> _entries = new();
    private WorldHost _host = null!;
    private readonly Random _random = new();

    public void Build(WorldHost host)
    {
        _host = host;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void Consume(List<SimEvent> events)
    {
        foreach (SimEvent e in events)
        {
            switch (e.Type)
            {
                case SimEventType.EnemyHit:
                case SimEventType.EnemyKilled:
                    Add(e, e.Crit ? UiKit.Ember : UiKit.Wax, e.Crit ? 22 : 15);
                    break;
                case SimEventType.HeroHit:
                    Add(e, MasterPalette.Instance["crimson4"], 20);
                    break;
            }
        }
    }

    private void Add(SimEvent e, Color color, int size)
    {
        if (_entries.Count >= Capacity)
        {
            _entries.RemoveAt(0);
        }
        float x = e.X.Raw / (float)Fix.OneRaw + (float)(_random.NextDouble() * 10 - 5);
        float y = e.Y.Raw / (float)Fix.OneRaw - 18;
        _entries.Add(new Entry { World = new Vector2(x, y), Text = e.Amount.ToString(CultureInfo.InvariantCulture), Color = color, Size = size });
    }

    public void Advance(float seconds)
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            Entry entry = _entries[i];
            entry.Age += seconds;
            if (entry.Age >= Life)
            {
                _entries.RemoveAt(i);
                continue;
            }
            _entries[i] = entry;
        }
        QueueRedraw();
    }

    public void Clear() => _entries.Clear();

    public override void _Draw()
    {
        Font font = ThemeDB.FallbackFont;
        Color outline = MasterPalette.Instance["ink"];
        foreach (Entry entry in _entries)
        {
            float t = entry.Age / Life;
            Vector2 at = _host.WorldToUi(entry.World) + new Vector2(-30, -28 * t);
            float alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
            float pop = t < 0.12f ? 1.25f - t * 2f : 1f;
            int size = (int)(entry.Size * pop);
            DrawStringOutline(font, at, entry.Text, HorizontalAlignment.Center, 60, size, 5, UiKit.WithAlpha(outline, alpha));
            DrawString(font, at, entry.Text, HorizontalAlignment.Center, 60, size, UiKit.WithAlpha(entry.Color, alpha));
        }
    }

    private struct Entry
    {
        public Vector2 World;
        public string Text;
        public Color Color;
        public int Size;
        public float Age;
    }
}
