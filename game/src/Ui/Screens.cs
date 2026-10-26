using System;
using System.Globalization;
using Glimmerfall.Sim;
using Godot;

namespace Glimmerfall.Game;

/// <summary>Титульный экран поверх живого демо-забега: название, слоган, «Новый забег / Настройки / Выход», подсказка управления.</summary>
public partial class TitleScreen : Control
{
    private Button _first = null!;

    public event Action? NewRun;
    public event Action? OpenSettings;
    public event Action? Quit;

    public void Build(Sfx sfx)
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(Vignette());

        var column = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center,
            AnchorTop = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = 96,
            OffsetRight = 96 + 760,
            OffsetTop = -250,
            OffsetBottom = 230,
        };
        column.AddThemeConstantOverride("separation", 14);
        var title = UiKit.Text("GLIMMERFALL", 84, UiKit.Glow, serif: true);
        title.AddThemeConstantOverride("outline_size", 10);
        column.AddChild(title);
        column.AddChild(UiKit.Text("Донеси свет сквозь Тьму", 26, UiKit.WaxDim, serif: true));
        column.AddChild(new Control { CustomMinimumSize = new Vector2(0, 28) });
        _first = UiKit.Button("Новый забег", () => NewRun?.Invoke(), sfx);
        column.AddChild(_first);
        column.AddChild(UiKit.Button("Настройки", () => OpenSettings?.Invoke(), sfx));
        column.AddChild(UiKit.Button("Выход", () => Quit?.Invoke(), sfx));
        foreach (Node child in column.GetChildren())
        {
            if (child is Button button)
            {
                button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            }
        }
        AddChild(column);

        var hint = UiKit.Text("WASD / стик — движение   ·   Пробел — рывок   ·   Q — смена героя   ·   E — ассист, затем Q — связка   ·   Esc — пауза", 16, UiKit.WaxDim);
        hint.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterBottom);
        hint.GrowHorizontal = GrowDirection.Both;
        hint.HorizontalAlignment = HorizontalAlignment.Center;
        hint.Position += new Vector2(0, -44);
        AddChild(hint);
        var version = UiKit.Text("прототип · заглушки вместо арта", 14, UiKit.WithAlpha(UiKit.WaxDim, 0.6f));
        version.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight);
        version.GrowHorizontal = GrowDirection.Begin;
        version.GrowVertical = GrowDirection.Begin;
        version.Position += new Vector2(-16, -12);
        AddChild(version);
    }

    public void FocusFirst() => _first.GrabFocus();

    /// <summary>Затемнение слева, где меню, и по краям: демо остаётся живым фоном, но не спорит с текстом.</summary>
    private static Control Vignette()
    {
        var gradient = new Gradient();
        gradient.SetColor(0, UiKit.WithAlpha(MasterPalette.Instance["ink"], 0.92f));
        gradient.SetColor(1, UiKit.WithAlpha(MasterPalette.Instance["ink"], 0.25f));
        var texture = new GradientTexture2D { Gradient = gradient, FillFrom = new Vector2(0, 0.5f), FillTo = new Vector2(0.75f, 0.5f), Width = 256, Height = 8 };
        var rect = new TextureRect
        {
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
            TextureFilter = TextureFilterEnum.Linear,
        };
        rect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        return rect;
    }
}

/// <summary>Настройки: сложность и длина забега (для нового забега), громкость, экран, тряска, FPS. Сохраняются сразу.</summary>
public partial class SettingsPanel : Control
{
    private GameSettings _settings = null!;
    private Button _back = null!;

    public event Action? Closed;
    public event Action? Changed;

    public void Build(GameSettings settings, Sfx sfx)
    {
        _settings = settings;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(UiKit.Scrim(0.55f));
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(560, 0) };
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        panel.AddChild(column);
        column.AddChild(UiKit.Text("Настройки", 34, UiKit.Glow, serif: true));

        var difficulty = new OptionButton();
        foreach (string name in GameSettings.DifficultyNames)
        {
            difficulty.AddItem(name);
        }
        difficulty.Selected = settings.Difficulty;
        difficulty.ItemSelected += index => Update(() => settings.Difficulty = (int)index, sfx);
        column.AddChild(Row("Сложность (новый забег)", difficulty));

        var length = new OptionButton();
        foreach (int acts in GameSettings.RunLengths)
        {
            length.AddItem(acts == 1 ? "1 акт · ~8 минут" : $"{acts} акта · ~{acts * 8} минут");
        }
        length.Selected = Array.IndexOf(GameSettings.RunLengths, settings.Acts);
        length.ItemSelected += index => Update(() => settings.Acts = GameSettings.RunLengths[(int)index], sfx);
        column.AddChild(Row("Длина забега", length));

        column.AddChild(Row("Общая громкость", Slider(settings.MasterVolume, v => settings.MasterVolume = v, sfx)));
        column.AddChild(Row("Эффекты", Slider(settings.EffectsVolume, v => settings.EffectsVolume = v, sfx)));
        column.AddChild(Toggle("Полный экран", settings.Fullscreen, on => settings.Fullscreen = on, sfx));
        column.AddChild(Toggle("Тряска камеры", settings.ScreenShake, on => settings.ScreenShake = on, sfx));
        column.AddChild(Toggle("Показывать FPS", settings.ShowFps, on => settings.ShowFps = on, sfx));
        column.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });
        _back = UiKit.Button("Назад", () => Closed?.Invoke(), sfx, 200);
        _back.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        column.AddChild(_back);
        AddChild(UiKit.Centered(panel));
    }

    public void FocusFirst() => _back.GrabFocus();

    private void Update(Action change, Sfx sfx)
    {
        change();
        _settings.Apply();
        _settings.Save();
        sfx.Play(Sound.UiClick, 0.6f, 0f);
        Changed?.Invoke();
    }

    private Control Slider(float value, Action<float> set, Sfx sfx)
    {
        var slider = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.05, Value = value, CustomMinimumSize = new Vector2(220, 24) };
        slider.ValueChanged += v => Update(() => set((float)v), sfx);
        return slider;
    }

    private Control Toggle(string text, bool value, Action<bool> set, Sfx sfx)
    {
        var box = new CheckBox { Text = text, ButtonPressed = value };
        box.Toggled += on => Update(() => set(on), sfx);
        return box;
    }

    private static Control Row(string label, Control control)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 16);
        var text = UiKit.Text(label, 18);
        text.CustomMinimumSize = new Vector2(250, 0);
        row.AddChild(text);
        control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(control);
        return row;
    }
}

/// <summary>Пауза: сим стоит, мир виден под затемнением.</summary>
public partial class PauseMenu : Control
{
    private Button _first = null!;

    public event Action? Resume;
    public event Action? OpenSettings;
    public event Action? ToTitle;

    public void Build(Sfx sfx)
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(UiKit.Scrim(0.5f));
        var panel = new PanelContainer();
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        panel.AddChild(column);
        var title = UiKit.Text("Пауза", 40, UiKit.Glow, serif: true);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(title);
        _first = UiKit.Button("Продолжить", () => Resume?.Invoke(), sfx);
        column.AddChild(_first);
        column.AddChild(UiKit.Button("Настройки", () => OpenSettings?.Invoke(), sfx));
        column.AddChild(UiKit.Button("В главное меню", () => ToTitle?.Invoke(), sfx));
        AddChild(UiKit.Centered(panel));
    }

    public void FocusFirst() => _first.GrabFocus();
}

/// <summary>Итог забега: победа или гибель отряда, короткая статистика, «Ещё раз» и «В меню».</summary>
public partial class ResultScreen : Control
{
    private Label _title = null!;
    private Label _subtitle = null!;
    private Label _stats = null!;
    private Button _first = null!;

    public event Action? Again;
    public event Action? ToTitle;

    public void Build(Sfx sfx)
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(UiKit.Scrim(0.6f));
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(520, 0) };
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        panel.AddChild(column);
        _title = UiKit.Text("", 48, UiKit.Glow, serif: true);
        _title.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(_title);
        _subtitle = UiKit.Text("", 20, UiKit.WaxDim, serif: true);
        _subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(_subtitle);
        _stats = UiKit.Text("", 20);
        column.AddChild(_stats);
        var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        buttons.AddThemeConstantOverride("separation", 16);
        _first = UiKit.Button("Ещё раз", () => Again?.Invoke(), sfx, 200);
        buttons.AddChild(_first);
        buttons.AddChild(UiKit.Button("В меню", () => ToTitle?.Invoke(), sfx, 200));
        column.AddChild(buttons);
        AddChild(UiKit.Centered(panel));
    }

    public void Show(World world, GameSettings settings)
    {
        bool victory = world.Outcome == RunOutcome.Victory;
        _title.Text = victory ? "Свет донесён" : "Отряд пал";
        _title.AddThemeColorOverride("font_color", victory ? UiKit.Glow : UiKit.Blood);
        int reached = world.ActIndex + 1;
        _subtitle.Text = victory
            ? $"Стражи мрака повержены · {GameSettings.DifficultyNames[settings.Difficulty]}"
            : $"Акт {reached} — {GameText.ActName(world.ActIndex)} · {GameSettings.DifficultyNames[settings.Difficulty]}";
        long damage = 0;
        foreach (Sim.Core.ActStats act in world.Stats.Acts)
        {
            damage += act.DamageDealt;
        }
        string killer = world.Stats.KillerKind >= 0 ? $"\nДобил: {GameText.EnemyNames[world.Stats.KillerKind]}" : "";
        _stats.Text = string.Format(CultureInfo.InvariantCulture,
            "Время: {0}\nВрагов повержено: {1}\nУровень отряда: {2}\nНанесено урона: {3}{4}",
            GameText.Clock(world.Tick), world.Stats.TotalKills, world.Level, damage, killer);
        Visible = true;
        _first.GrabFocus();
    }
}
