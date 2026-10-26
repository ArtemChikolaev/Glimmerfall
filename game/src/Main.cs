using System;
using System.Globalization;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Content;
using Glimmerfall.Sim.Determinism;
using Glimmerfall.Sim.Input;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Корень спайка орды (BACKLOG G1.3). Сим шагает фиксированным тиком 60 Гц из аккумулятора кадра, рендер идёт на частоте
/// дисплея с интерполяцией между тиками. Ввод — автопилот-кайтер из Sim.dll или игрок (F1 переключает).
/// Режимы: --bench-seconds N (замер кадра и отчёт), --smoke-frames N (headless-проверка в CI, код выхода).
/// </summary>
public partial class Main : Node2D
{
    private const double TickSeconds = 1.0 / SimConstants.TicksPerSecond;
    private const int MaxStepsPerFrame = 6;

    private SpikeOptions _options = new();
    private World _world = null!;
    private IBotPolicy? _autopilot;
    private WorldView _view = null!;
    private Camera2D _camera = null!;
    private Label _hud = null!;
    private TouchControls _touch = null!;
    private FrameStats _stats = new();
    private FrameStats _hudStats = new();
    private double _accumulator;
    private double _elapsed;
    private double _hudTimer;
    private int _frames;
    private bool _toggleHeld;
    private bool _failed;
    private string? _benchReport;
    private Rid _viewport;
    private float _gridScale = 1f;
    private int _captureStage;

    public override void _Ready()
    {
        try
        {
            _options = SpikeOptions.FromEnvironment(OS.GetCmdlineUserArgs(), OS.HasFeature("bench"));
        }
        catch (ArgumentException e)
        {
            GD.PrintErr(e.Message);
            _failed = true;
            GetTree().Quit(2);
            return;
        }
        SimConfig config = _options.Scenario == "run" ? SimConfig.Run(_options.Acts) : SimConfig.HordeBench(_options.Enemies, _options.Projectiles);
        _world = new World(config, SeedHash.FromString(_options.Seed)) { CollectEvents = true };
        _autopilot = _options.Autopilot ? new KiterPolicy() : null;

        // Сетка: мир всегда 640×360 единиц сима; на 960×540 камера увеличивает его в 1,5 раза, а листы _960 рисуются
        // тем же размером в мире — тексель ложится в пиксель обеих сеток.
        _gridScale = _options.Grid / 640f;
        GetTree().Root.ContentScaleSize = new Vector2I(_options.Grid, _options.Grid * 9 / 16);
        _view = new WorldView();
        AddChild(_view);
        _view.Init(_world, _gridScale, _options.Screenshot != null ? 7 : null);
        _camera = new Camera2D { Zoom = new Vector2(_gridScale, _gridScale) };
        AddChild(_camera);
        _camera.MakeCurrent();
        var layer = new CanvasLayer();
        AddChild(layer);
        _hud = new Label { Position = new Vector2(4, 2), Visible = _options.Screenshot == null };
        _hud.AddThemeFontSizeOverride("font_size", 8);
        _hud.AddThemeColorOverride("font_shadow_color", Colors.Black);
        layer.AddChild(_hud);
        _touch = new TouchControls { Visible = _options.Screenshot == null };
        layer.AddChild(_touch);

        _viewport = GetViewport().GetViewportRid();
        RenderingServer.ViewportSetMeasureRenderTime(_viewport, true);
        if (!_options.Vsync)
        {
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            Engine.MaxFps = 0;
        }
    }

    public override void _Process(double delta)
    {
        if (_failed)
        {
            return;
        }
        try
        {
            Frame(delta);
        }
        catch (Exception e)
        {
            _failed = true;
            GD.PrintErr($"spike failed: {e}");
            GetTree().Quit(1);
        }
    }

    private void Frame(double delta)
    {
        if (_captureStage > 0)
        {
            Capture();
            return;
        }
        ulong frameStart = Time.GetTicksUsec();
        HandleToggle();
        _accumulator += Math.Min(delta, 0.25);
        ulong simUs = 0;
        int steps = 0;
        _touch.SetModal(_autopilot == null && _world.Modal != ModalKind.None ? _world.ModalOptionCount : 0);
        while (_accumulator >= TickSeconds && steps < MaxStepsPerFrame)
        {
            InputFrame input = _autopilot?.Decide(_world) ?? PlayerInput.Read(_world, _touch);
            ulong t0 = Time.GetTicksUsec();
            _world.Step(input);
            simUs += Time.GetTicksUsec() - t0;
            _view.Consume(_world.Events);
            _accumulator -= TickSeconds;
            steps++;
        }
        if (steps == MaxStepsPerFrame)
        {
            _accumulator = 0;
        }
        float alpha = _world.Modal == ModalKind.None ? (float)(_accumulator / TickSeconds) : 1f;
        _view.Render(alpha, (float)delta);
        _camera.Position = (_view.HeroPosition * _gridScale).Round() / _gridScale;
        double scriptMs = (Time.GetTicksUsec() - frameStart) / 1000.0;

        _frames++;
        _elapsed += delta;
        double gpu = RenderingServer.ViewportGetMeasuredRenderTimeGpu(_viewport);
        double cpu = RenderingServer.ViewportGetMeasuredRenderTimeCpu(_viewport);
        bool measuring = _options.BenchSeconds > 0 && _elapsed >= _options.WarmupSeconds;
        if (measuring)
        {
            _stats.Record(delta * 1000.0, scriptMs, simUs / 1000.0, cpu, gpu, _world, _view.DrawnInstances);
        }
        _hudStats.Record(delta * 1000.0, scriptMs, simUs / 1000.0, cpu, gpu, _world, _view.DrawnInstances);
        _hudTimer += delta;
        if (_hudTimer >= 0.25)
        {
            UpdateHud();
            _hudTimer = 0;
            _hudStats = new FrameStats();
        }
        if (measuring && _elapsed >= _options.WarmupSeconds + _options.BenchSeconds)
        {
            FinishBench();
        }
        if (_options.SmokeFrames > 0 && _frames >= _options.SmokeFrames)
        {
            FinishSmoke();
        }
        if (_options.Screenshot != null && _frames == _options.ScreenshotFrame)
        {
            _captureStage = 1;
        }
    }

    /// <summary>
    /// Снимок для сравнения сеток и проверки читаемости (G2.6): кадр сцены, затем тот же кадр маской силуэтов
    /// (герой — красный, рядовые — зелёный, элиты — синий). Сим на время съёмки стоит. Картинки — во внутреннем
    /// разрешении сетки; анализ — `glimmer_sprites readability`.
    /// </summary>
    private void Capture()
    {
        Image image = GetViewport().GetTexture().GetImage();
        string prefix = System.IO.Path.GetFullPath(_options.Screenshot!);
        if (_captureStage == 1)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefix)!);
            image.SavePng(prefix + ".png");
            _view.SetMaskMode(true);
            _captureStage = 2;
            return;
        }
        image.SavePng(prefix + ".mask.png");
        string info = string.Format(CultureInfo.InvariantCulture,
            "{{\"grid\": [{0}, {1}], \"frame\": {2}, \"tick\": {3}, \"enemies\": {4}, \"visible_enemies\": {5}, \"seed\": \"{6}\"}}\n",
            image.GetWidth(), image.GetHeight(), _frames, _world.Tick, _world.Enemies.Count, _view.VisibleEnemies, _options.Seed);
        using (FileAccess file = FileAccess.Open(prefix + ".json", FileAccess.ModeFlags.Write))
        {
            file?.StoreString(info);
        }
        GD.Print($"screenshot {prefix}.png + .mask.png: {info.Trim()}");
        GetTree().Quit(0);
    }

    private void HandleToggle()
    {
        bool held = Input.IsKeyPressed(Key.F1);
        if ((held && !_toggleHeld) || _touch.TakeAutopilotToggle())
        {
            _autopilot = _autopilot == null ? new KiterPolicy() : null;
        }
        _toggleHeld = held;
    }

    private void UpdateHud()
    {
        Sim.Core.Squad s = _world.Squad;
        HeroKind hero = _world.ActiveHero;
        string modal = _world.Modal switch
        {
            ModalKind.Blessing => "  ВЫБОР: " + Options(k => ((Blessing)_world.ModalOption(k)).ToString()),
            ModalKind.Checkpoint => "  ЧЕКПОИНТ: " + Options(k => ((CheckpointOption)_world.ModalOption(k)).ToString()),
            _ => "",
        };
        if (_benchReport != null)
        {
            _hud.Text = _benchReport;
            return;
        }
        _hud.Text = string.Format(CultureInfo.InvariantCulture,
            "{0:0} fps · {1} · tick {2} · act {3} {4}:{5:00}\nenemies {6} · projectiles {7} · shots {8} · instances {9}\n{10} hp {11}/{12} · squad {13}/3 · lvl {14} · {15} · {16}{17}",
            Engine.GetFramesPerSecond(), _hudStats.Compact(), _world.Tick, _world.ActIndex + 1,
            _world.ActTick / 3600, _world.ActTick / 60 % 60, _world.Enemies.Count, _world.Projectiles.Count, _world.Shots.Count,
            _view.DrawnInstances, hero.Name, s.Hp[s.Active], s.MaxHp[s.Active], s.AliveCount, _world.Level,
            _autopilot == null ? "manual (F1 autopilot)" : "autopilot (F1 manual)", _world.Outcome == RunOutcome.InProgress ? "" : _world.Outcome.ToString(), modal);
    }

    private string Options(Func<int, string> name)
    {
        var parts = new string[_world.ModalOptionCount];
        for (int k = 0; k < parts.Length; k++)
        {
            parts[k] = $"[{k + 1}] {name(k)}";
        }
        return string.Join("  ", parts);
    }

    private void FinishBench()
    {
        string report = _stats.Report(_options);
        GD.Print(report);
        string name = $"user://bench-{_options.Scenario}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";
        using (FileAccess file = FileAccess.Open(name, FileAccess.ModeFlags.Write))
        {
            file?.StoreString(report);
        }
        GD.Print($"bench report saved to {ProjectSettings.GlobalizePath(name)}");
        if (_options.QuitAfterBench)
        {
            GetTree().Quit(0);
            return;
        }
        _benchReport = report;
        _options.BenchSeconds = 0;
    }

    private void FinishSmoke()
    {
        bool ok = _world.Tick > 0 && _world.Enemies.Count > 0 && _view.DrawnInstances > 0 && _world.Outcome == RunOutcome.InProgress;
        GD.Print($"smoke {(ok ? "ok" : "FAILED")}: frames {_frames} · ticks {_world.Tick} · enemies {_world.Enemies.Count} · projectiles {_world.Projectiles.Count} · instances {_view.DrawnInstances} · digest {_world.ComputeDigest():x16}");
        GetTree().Quit(ok ? 0 : 1);
    }
}
