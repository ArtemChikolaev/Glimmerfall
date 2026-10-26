using System;
using System.Collections.Generic;
using System.Globalization;
using Glimmerfall.Sim;
using Glimmerfall.Sim.Bots;
using Glimmerfall.Sim.Core;
using Glimmerfall.Sim.Input;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Корень игры. Без аргументов — титульный экран поверх живого демо-забега, настройки, забег с HUD и выбором
/// благословений, пауза (Esc), итог. Служебные режимы (<see cref="SpikeOptions.Direct"/>) идут мимо меню: замер кадра,
/// смоук CI, снимки для проверки читаемости, спайк орды в окне.
/// </summary>
public partial class Main : Node
{
    private enum Mode
    {
        Title,
        Run,
        Paused,
        Result,
        Direct,
    }

    private const float ResultDelaySeconds = 2.2f;

    private SpikeOptions _options = new();
    private GameSettings _settings = new();
    private WorldHost _host = null!;
    private RunSession? _session;
    private Sfx _sfx = null!;
    private Control _ui = null!;
    private TitleScreen _title = null!;
    private SettingsPanel _settingsPanel = null!;
    private PauseMenu _pause = null!;
    private ResultScreen _result = null!;
    private Hud _hud = null!;
    private ChoiceCards _cards = null!;
    private Banner _banner = null!;
    private DamageNumbers _numbers = null!;
    private TouchControls _touch = null!;
    private Label _debug = null!;
    private Mode _mode;
    private Mode _settingsReturn;
    private int _pendingPick = -1;
    private float _resultTimer;
    private float _demoRestart;
    private int _seenAct = -1;
    private bool _seenBoss;
    private int _seenXp;
    private float _trauma;
    private readonly Random _shakeRandom = new();

    private FrameStats _stats = new();
    private FrameStats _debugStats = new();
    private double _elapsed;
    private double _debugTimer;
    private int _frames;
    private bool _toggleHeld;
    private bool _failed;
    private string? _benchReport;
    private Rid _viewportRid;
    private int _captureStage;
    private bool _uiCaptured;

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
        _settings = GameSettings.Load();
        _host = WorldHost.Create(this, _options.Grid / 640f);
        _sfx = new Sfx { Name = "Sfx" };
        AddChild(_sfx);
        if (_options.Direct)
        {
            Sfx.SetVolumes(_settings.MasterVolume, _settings.EffectsVolume);
        }
        else
        {
            _settings.Apply();
        }
        BuildUi();

        _viewportRid = GetViewport().GetViewportRid();
        RenderingServer.ViewportSetMeasureRenderTime(_viewportRid, true);
        if (!_options.Vsync)
        {
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            Engine.MaxFps = 0;
        }

        if (_options.Direct)
        {
            StartDirect();
        }
        else if (_options.Autostart)
        {
            StartRun();
        }
        else
        {
            ShowTitle();
        }
    }

    private void BuildUi()
    {
        var layer = new CanvasLayer { Name = "Ui", Layer = 10 };
        AddChild(layer);
        _ui = new Control { Theme = UiKit.Theme, MouseFilter = Control.MouseFilterEnum.Ignore, TextureFilter = CanvasItem.TextureFilterEnum.Linear };
        _ui.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(_ui);

        _hud = new Hud();
        _ui.AddChild(_hud);
        _hud.Build(SpriteSheet.Load(_options.Grid == 960 ? "lamplighter_960" : "lamplighter"));
        _numbers = new DamageNumbers();
        _ui.AddChild(_numbers);
        _numbers.Build(_host);
        _banner = new Banner();
        _ui.AddChild(_banner);
        _banner.Build();
        _touch = new TouchControls();
        _ui.AddChild(_touch);
        _cards = new ChoiceCards();
        _ui.AddChild(_cards);
        _cards.Build(_sfx);
        _cards.Chosen += index => _pendingPick = index;
        _pause = new PauseMenu();
        _ui.AddChild(_pause);
        _pause.Build(_sfx);
        _pause.Resume += Resume;
        _pause.OpenSettings += () => OpenSettings(Mode.Paused);
        _pause.ToTitle += ShowTitle;
        _result = new ResultScreen();
        _ui.AddChild(_result);
        _result.Build(_sfx);
        _result.Again += StartRun;
        _result.ToTitle += ShowTitle;
        _title = new TitleScreen();
        _ui.AddChild(_title);
        _title.Build(_sfx);
        _title.NewRun += StartRun;
        _title.OpenSettings += () => OpenSettings(Mode.Title);
        _title.Quit += () => GetTree().Quit(0);
        _settingsPanel = new SettingsPanel();
        _ui.AddChild(_settingsPanel);
        _settingsPanel.Build(_settings, _sfx);
        _settingsPanel.Closed += CloseSettings;
        _settingsPanel.Changed += () => _hud.ShowFps = _settings.ShowFps;
        _debug = new Label { Position = new Vector2(8, 6) };
        _debug.AddThemeFontSizeOverride("font_size", 14);
        _ui.AddChild(_debug);
        HideAll();
    }

    private void HideAll()
    {
        foreach (Control control in new Control[] { _hud, _numbers, _banner, _touch, _cards, _pause, _result, _title, _settingsPanel, _debug })
        {
            control.Visible = false;
        }
    }

    // ---------- переходы ----------

    private void StartSession(SimConfig config, string seed, bool autopilot)
    {
        _session?.Dispose();
        _session = new RunSession(config, seed, _host, _options.Screenshot != null || _options.CaptureUi != null ? 7 : null, autopilot);
        _numbers.Clear();
        _seenAct = -1;
        _seenBoss = false;
        _seenXp = 0;
        _trauma = 0;
        _pendingPick = -1;
        _resultTimer = 0;
    }

    private void ShowTitle()
    {
        HideAll();
        _mode = Mode.Title;
        _sfx.Muted = true;
        var config = SimConfig.Run(3);
        StartSession(config, "demo-" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture), autopilot: true);
        _title.Visible = true;
        _title.FocusFirst();
    }

    private void StartRun()
    {
        HideAll();
        _mode = Mode.Run;
        _sfx.Muted = false;
        SimConfig config = SimConfig.Run(_settings.Acts);
        config.Tier = _settings.Difficulty;
        bool reproducible = _options.Autostart || _options.CaptureUi != null || _options.SmokeFlowFrames > 0;
        StartSession(config, reproducible ? _options.Seed : "run-" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture), _options.RunAutopilot);
        _hud.Visible = true;
        _hud.ShowFps = _settings.ShowFps;
        _numbers.Visible = true;
        _banner.Visible = false;
        _touch.Visible = true;
    }

    private void StartDirect()
    {
        HideAll();
        _mode = Mode.Direct;
        SimConfig config = _options.Scenario == "run" ? SimConfig.Run(_options.Acts) : SimConfig.HordeBench(_options.Enemies, _options.Projectiles);
        StartSession(config, _options.Seed, _options.Autopilot);
        _debug.Visible = _options.Screenshot == null;
        _touch.Visible = _options.Screenshot == null;
    }

    private void Pause()
    {
        if (_mode != Mode.Run)
        {
            return;
        }
        _mode = Mode.Paused;
        _pause.Visible = true;
        _cards.Visible = false;
        _pause.FocusFirst();
    }

    private void Resume()
    {
        _pause.Visible = false;
        _mode = Mode.Run;
    }

    private void OpenSettings(Mode from)
    {
        _settingsReturn = from;
        _title.Visible = false;
        _pause.Visible = false;
        _settingsPanel.Visible = true;
        _settingsPanel.FocusFirst();
    }

    private void CloseSettings()
    {
        _settingsPanel.Visible = false;
        if (_settingsReturn == Mode.Title)
        {
            _title.Visible = true;
            _title.FocusFirst();
        }
        else
        {
            _pause.Visible = true;
            _pause.FocusFirst();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_mode == Mode.Direct)
        {
            return;
        }
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (_settingsPanel.Visible)
            {
                CloseSettings();
            }
            else if (_mode == Mode.Run)
            {
                Pause();
            }
            else if (_mode == Mode.Paused)
            {
                Resume();
            }
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event is InputEventKey { Pressed: true, Echo: false } key && _cards.Visible && key.Keycode is >= Key.Key1 and <= Key.Key3)
        {
            _cards.Choose((int)(key.Keycode - Key.Key1));
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut && _mode == Mode.Run && _options.CaptureUi == null && _options.SmokeFlowFrames == 0)
        {
            Pause();
        }
    }

    // ---------- кадр ----------

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
            GD.PrintErr($"game failed: {e}");
            GetTree().Quit(1);
        }
    }

    private void Frame(double delta)
    {
        if (_captureStage > 0)
        {
            CaptureWorld();
            return;
        }
        if (_session == null)
        {
            return;
        }
        ulong frameStart = Time.GetTicksUsec();
        float seconds = (float)delta;
        double simMs = 0;
        World world = _session.World;
        switch (_mode)
        {
            case Mode.Title:
                _session.Advance(delta, _ => default, OnEvents, out simMs);
                if (world.Outcome != RunOutcome.InProgress && (_demoRestart += seconds) > 2f)
                {
                    _demoRestart = 0;
                    StartSession(SimConfig.Run(3), "demo-" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture), autopilot: true);
                }
                break;
            case Mode.Run:
                HandleAutopilotToggle();
                // снимки и смоук ведут забег ботом, но выбор в модалке делают через карточки — как игрок
                bool cardsForBot = _options.CaptureCards || _options.SmokeFlowFrames > 0;
                if (world.Modal == ModalKind.None)
                {
                    _cards.Visible = false;
                    if (cardsForBot && _options.RunAutopilot && _session.Autopilot == null)
                    {
                        _session.Autopilot = new KiterPolicy();
                    }
                }
                else if (_pendingPick < 0 && (_session.Autopilot == null || cardsForBot))
                {
                    _cards.Show(world);
                    if (cardsForBot)
                    {
                        _session.Autopilot = null;
                    }
                }
                _session.Advance(delta, w =>
                {
                    InputFrame input = PlayerInput.Read(w, _touch, _pendingPick);
                    if (input.Pick != 0)
                    {
                        _pendingPick = -1;
                    }
                    return input;
                }, OnEvents, out simMs);
                TrackRun(world, seconds);
                break;
            case Mode.Direct:
                HandleAutopilotToggle();
                _session.Advance(delta, w => PlayerInput.Read(w, _touch, -1), OnEvents, out simMs);
                break;
        }
        bool frozen = _mode is Mode.Paused or Mode.Result;
        _session.View.Render(_session.Alpha, frozen ? 0f : seconds);
        UpdateCamera(seconds);
        if (_mode is Mode.Run or Mode.Paused or Mode.Result)
        {
            _hud.Refresh(world, frozen ? 0f : seconds);
            _numbers.Advance(frozen ? 0f : seconds);
        }
        Measure(delta, (Time.GetTicksUsec() - frameStart) / 1000.0, simMs);
        CheckAutomation();
    }

    private void HandleAutopilotToggle()
    {
        bool held = Input.IsKeyPressed(Key.F1);
        if (_session != null && ((held && !_toggleHeld) || _touch.TakeAutopilotToggle()))
        {
            _session.Autopilot = _session.Autopilot == null ? new KiterPolicy() : null;
        }
        _toggleHeld = held;
    }

    /// <summary>События тика: частицы и трупы, звук, цифры урона, тряска, надписи.</summary>
    private void OnEvents(List<SimEvent> events)
    {
        _session!.View.Consume(events);
        _sfx.Consume(events);
        if (_mode != Mode.Run)
        {
            return;
        }
        _numbers.Consume(events);
        foreach (SimEvent e in events)
        {
            switch (e.Type)
            {
                case SimEventType.HeroHit:
                    _hud.Hurt(0.55f);
                    AddTrauma(0.3f);
                    break;
                case SimEventType.HeroDown:
                    World world = _session.World;
                    int next = world.Squad.Kind[world.Squad.Active];
                    _banner.Show($"{GameText.HeroNames[e.Kind]} пал", world.Squad.AliveCount > 0 ? $"В бой вступает {GameText.HeroNames[next]}" : "", 2.2f, UiKit.Blood);
                    AddTrauma(0.6f);
                    break;
                case SimEventType.Slam:
                    AddTrauma(0.7f);
                    break;
                case SimEventType.EnemyKilled when e.Kind >= Sim.Content.EnemyIds.Ravager:
                    AddTrauma(0.45f);
                    break;
            }
        }
    }

    /// <summary>Надписи начала акта и появления босса, итог забега с паузой на анимацию смерти.</summary>
    private void TrackRun(World world, float seconds)
    {
        if (world.ActIndex != _seenAct && world.Modal == ModalKind.None)
        {
            _seenAct = world.ActIndex;
            _seenBoss = false;
            _banner.Show($"Акт {world.ActIndex + 1} — {GameText.ActName(world.ActIndex)}", "Продержись до прихода Стража мрака и победи его", 4f);
        }
        if (world.IsBossAlive && !_seenBoss)
        {
            _seenBoss = true;
            _banner.Show("Страж мрака пробудился!", "Победи его, чтобы закрыть акт", 3f, MasterPalette.Instance["telegraph2"]);
            _sfx.Play(Sound.BossSpawn, 1f, 0f);
            AddTrauma(0.5f);
        }
        if (world.Xp > _seenXp || world.Level > 1 && world.Xp < _seenXp)
        {
            _sfx.Play(Sound.Pickup, 0.35f, 0.1f);
        }
        _seenXp = world.Xp;
        if (world.Outcome == RunOutcome.InProgress)
        {
            return;
        }
        _resultTimer += seconds;
        if (_resultTimer < ResultDelaySeconds)
        {
            return;
        }
        _mode = Mode.Result;
        _cards.Visible = false;
        _sfx.Play(world.Outcome == RunOutcome.Victory ? Sound.Victory : Sound.Defeat, 1f, 0f);
        _result.Show(world, _settings);
    }

    private void AddTrauma(float amount) => _trauma = Math.Min(1f, _trauma + amount);

    private void UpdateCamera(float seconds)
    {
        _trauma = Math.Max(0f, _trauma - seconds * 1.6f);
        Vector2 shake = Vector2.Zero;
        if (_settings.ScreenShake && _mode == Mode.Run && _trauma > 0f)
        {
            float power = _trauma * _trauma * 6f;
            shake = new Vector2((float)(_shakeRandom.NextDouble() * 2 - 1), (float)(_shakeRandom.NextDouble() * 2 - 1)) * power;
        }
        _host.LookAt(_session!.View.HeroPosition, shake);
    }

    // ---------- замеры, смоук, снимки ----------

    private void Measure(double delta, double scriptMs, double simMs)
    {
        _frames++;
        _elapsed += delta;
        World world = _session!.World;
        double gpu = RenderingServer.ViewportGetMeasuredRenderTimeGpu(_viewportRid);
        double cpu = RenderingServer.ViewportGetMeasuredRenderTimeCpu(_viewportRid);
        bool measuring = _options.BenchSeconds > 0 && _elapsed >= _options.WarmupSeconds;
        if (measuring)
        {
            _stats.Record(delta * 1000.0, scriptMs, simMs, cpu, gpu, world, _session.View.DrawnInstances);
        }
        if (_mode != Mode.Direct)
        {
            return;
        }
        _debugStats.Record(delta * 1000.0, scriptMs, simMs, cpu, gpu, world, _session.View.DrawnInstances);
        _debugTimer += delta;
        if (_debugTimer >= 0.25)
        {
            _debug.Text = _benchReport ?? string.Format(CultureInfo.InvariantCulture,
                "{0:0} fps · {1} · tick {2} · enemies {3} · projectiles {4} · instances {5} · {6}",
                Engine.GetFramesPerSecond(), _debugStats.Compact(), world.Tick, world.Enemies.Count, world.Projectiles.Count,
                _session.View.DrawnInstances, _session.Autopilot == null ? "manual (F1 autopilot)" : "autopilot (F1 manual)");
            _debugTimer = 0;
            _debugStats = new FrameStats();
        }
        if (measuring && _elapsed >= _options.WarmupSeconds + _options.BenchSeconds)
        {
            FinishBench();
        }
    }

    private void CheckAutomation()
    {
        if (_options.SmokeFrames > 0 && _frames >= _options.SmokeFrames)
        {
            FinishSmoke();
        }
        if (_options.Screenshot != null && _frames == _options.ScreenshotFrame)
        {
            _captureStage = 1;
        }
        if (_options.SmokeFlowFrames > 0)
        {
            SmokeFlow();
        }
        bool cardsReady = _options.CaptureCards && _cards.Visible && _frames > 10;
        if (_options.CaptureUi != null && !_uiCaptured && (cardsReady || !_options.CaptureCards && _frames >= _options.CaptureFrame))
        {
            _uiCaptured = true;
            switch (_options.CaptureScreen)
            {
                case "pause":
                    Pause();
                    break;
                case "settings":
                    Pause();
                    OpenSettings(Mode.Paused);
                    break;
                case "result":
                    _mode = Mode.Result;
                    _result.Show(_session!.World, _settings);
                    break;
            }
            CallDeferred(MethodName.CaptureWindow);
        }
    }

    /// <summary>Снимок окна с интерфейсом — после отрисовки кадра (кадр, следующий за этим).</summary>
    private async void CaptureWindow()
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        string path = System.IO.Path.GetFullPath(_options.CaptureUi!) + ".png";
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        GetViewport().GetTexture().GetImage().SavePng(path);
        GD.Print($"capture-ui {path}: frame {_frames}, mode {_mode}, tick {_session?.World.Tick}");
        GetTree().Quit(0);
    }

    /// <summary>CI: титул → забег (с выбором в модалках) → пауза → забег; вердикт по тикам и состоянию UI.</summary>
    private void SmokeFlow()
    {
        switch (_frames)
        {
            case 10:
                _options.RunAutopilot = true;
                StartRun();
                break;
            case 40:
                Pause();
                break;
            case 50:
                Resume();
                break;
        }
        if (_mode == Mode.Run && _cards.Visible && _frames % 5 == 0)
        {
            _cards.Choose(0);
        }
        if (_frames < _options.SmokeFlowFrames)
        {
            return;
        }
        World world = _session!.World;
        bool ok = _mode is Mode.Run or Mode.Result && world.Tick > 0 && _hud.Visible && _session.View.DrawnInstances > 0 && world.Level > 1;
        GD.Print($"smoke-flow {(ok ? "ok" : "FAILED")}: frames {_frames} · mode {_mode} · ticks {world.Tick} · level {world.Level} · enemies {world.Enemies.Count} · kills {world.Stats.TotalKills}");
        GetTree().Quit(ok ? 0 : 1);
    }

    /// <summary>
    /// Снимок для сравнения сеток и проверки читаемости (G2.6): кадр мира во внутреннем разрешении, затем тот же кадр
    /// маской силуэтов (герой — красный, рядовые — зелёный, элиты — синий). Сим на время съёмки стоит.
    /// </summary>
    private void CaptureWorld()
    {
        Image image = _host.CaptureWorld();
        string prefix = System.IO.Path.GetFullPath(_options.Screenshot!);
        if (_captureStage == 1)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefix)!);
            image.SavePng(prefix + ".png");
            _session!.View.SetMaskMode(true);
            _host.Darkness.Visible = false;
            _captureStage = 2;
            return;
        }
        image.SavePng(prefix + ".mask.png");
        string info = string.Format(CultureInfo.InvariantCulture,
            "{{\"grid\": [{0}, {1}], \"frame\": {2}, \"tick\": {3}, \"enemies\": {4}, \"visible_enemies\": {5}, \"seed\": \"{6}\"}}\n",
            image.GetWidth(), image.GetHeight(), _frames, _session!.World.Tick, _session.World.Enemies.Count, _session.View.VisibleEnemies, _options.Seed);
        using (FileAccess file = FileAccess.Open(prefix + ".json", FileAccess.ModeFlags.Write))
        {
            file?.StoreString(info);
        }
        GD.Print($"screenshot {prefix}.png + .mask.png: {info.Trim()}");
        GetTree().Quit(0);
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
        World world = _session!.World;
        bool ok = world.Tick > 0 && world.Enemies.Count > 0 && _session.View.DrawnInstances > 0 && world.Outcome == RunOutcome.InProgress;
        GD.Print($"smoke {(ok ? "ok" : "FAILED")}: frames {_frames} · ticks {world.Tick} · enemies {world.Enemies.Count} · projectiles {world.Projectiles.Count} · instances {_session.View.DrawnInstances} · digest {world.ComputeDigest():x16}");
        GetTree().Quit(ok ? 0 : 1);
    }
}
