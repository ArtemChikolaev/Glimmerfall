using System;
using System.Globalization;

namespace Glimmerfall.Game;

/// <summary>
/// Пользовательские аргументы после «--». Без аргументов — игра: титульный экран, меню, забег.
/// Служебные режимы идут мимо меню: <c>--bench-seconds 60</c> (замер кадра), <c>--smoke-frames 240</c> (CI),
/// <c>--scenario horde</c> (спайк орды в окне), <c>--scenario run --manual</c> (сразу забег),
/// <c>--grid 960 --screenshot build/shot</c> (снимок мира и маски силуэтов, G2.6);
/// <c>--capture-ui PATH</c> — снимок окна с интерфейсом, <c>--smoke-flow N</c> — CI-проверка пути меню → забег → пауза.
/// </summary>
public sealed class SpikeOptions
{
    /// <summary>Пусто — игра с меню; «horde» — спайк орды; «run» — сразу забег.</summary>
    public string Scenario = "";
    public int Enemies = 5000;
    public int Projectiles = 2000;
    public int Acts = 1;
    public string Seed = "spike";
    public bool Autopilot = true;
    public int BenchSeconds;
    public int WarmupSeconds = 5;
    public int SmokeFrames;
    public bool Vsync = true;

    /// <summary>Внутреннее разрешение: 640 (640×360, ADR 0003) или 960 (960×540) — сравнение сеток G2.6.</summary>
    public int Grid = 640;

    /// <summary>Снимок кадра и маски силуэтов (G2.6): путь без расширения, кадр съёмки.</summary>
    public string? Screenshot;
    public int ScreenshotFrame = 600;

    /// <summary>Снимок всего окна с интерфейсом (для отчётов): путь без расширения и кадр.</summary>
    public string? CaptureUi;
    public int CaptureFrame = 120;

    /// <summary>Снять окно, когда откроется выбор благословения (автопилот играет до модалки).</summary>
    public bool CaptureCards;

    /// <summary>Перед снимком открыть экран: pause, settings или result — проверка раскладки.</summary>
    public string? CaptureScreen;

    /// <summary>Снять окно через <see cref="CaptureDelay"/> кадров после N-го ассиста или N-го дуэта связки (проверка глазами).</summary>
    public int CaptureAssist;
    public int CaptureDuet;
    public int CaptureDelay = 6;

    /// <summary>Игра: сразу начать забег (мимо титула); с <see cref="RunAutopilot"/> — забег ведёт бот.</summary>
    public bool Autostart;
    public bool RunAutopilot;

    /// <summary>Headless-проверка пути «титул → забег → пауза → забег» с выбором в модалках: кадров до вердикта.</summary>
    public int SmokeFlowFrames;

    /// <summary>Замер закончился — выйти (ПК, CI) или оставить отчёт на экране для скриншота (телефон, Deck в игровом режиме).</summary>
    public bool QuitAfterBench = true;

    /// <summary>Служебный режим мимо меню: замер, смоук, снимок мира или явный сценарий.</summary>
    public bool Direct => BenchSeconds > 0 || SmokeFrames > 0 || Screenshot != null || Scenario.Length > 0;

    /// <summary>
    /// Сборка с тегом «bench» (пресеты «Android Bench», «iOS Bench») без аргументов сразу гонит замер: на телефоне
    /// аргументы командной строки неудобны.
    /// </summary>
    public static SpikeOptions FromEnvironment(string[] args, bool benchFeature)
    {
        SpikeOptions options = Parse(args);
        if (benchFeature && args.Length == 0)
        {
            options.BenchSeconds = 60;
            options.Vsync = false;
            options.QuitAfterBench = false;
            options.Scenario = "horde";
        }
        return options;
    }

    public static SpikeOptions Parse(string[] args)
    {
        var options = new SpikeOptions();
        for (int i = 0; i < args.Length; i++)
        {
            string key = args[i];
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"{key} needs a value");
            int NextInt() => int.Parse(Next(), NumberStyles.Integer, CultureInfo.InvariantCulture);
            switch (key)
            {
                case "--scenario": options.Scenario = Next(); break;
                case "--enemies": options.Enemies = NextInt(); break;
                case "--projectiles": options.Projectiles = NextInt(); break;
                case "--acts": options.Acts = NextInt(); break;
                case "--seed": options.Seed = Next(); break;
                case "--manual": options.Autopilot = false; break;
                case "--bench-seconds": options.BenchSeconds = NextInt(); break;
                case "--warmup-seconds": options.WarmupSeconds = NextInt(); break;
                case "--smoke-frames": options.SmokeFrames = NextInt(); break;
                case "--no-vsync": options.Vsync = false; break;
                case "--grid": options.Grid = NextInt(); break;
                case "--screenshot": options.Screenshot = Next(); break;
                case "--screenshot-frame": options.ScreenshotFrame = NextInt(); break;
                case "--capture-ui": options.CaptureUi = Next(); break;
                case "--capture-frame": options.CaptureFrame = NextInt(); break;
                case "--capture-cards": options.CaptureCards = true; break;
                case "--capture-screen": options.CaptureScreen = Next(); break;
                case "--capture-assist": options.CaptureAssist = NextInt(); break;
                case "--capture-duet": options.CaptureDuet = NextInt(); break;
                case "--capture-delay": options.CaptureDelay = NextInt(); break;
                case "--autostart": options.Autostart = true; break;
                case "--autopilot": options.RunAutopilot = true; break;
                case "--smoke-flow": options.SmokeFlowFrames = NextInt(); break;
                default: throw new ArgumentException($"unknown option {key}");
            }
        }
        if (options.Grid is not (640 or 960))
        {
            throw new ArgumentException("--grid: 640 или 960");
        }
        if (options.Scenario is not ("" or "horde" or "run"))
        {
            throw new ArgumentException("--scenario: horde или run");
        }
        if (options.Direct && options.Scenario.Length == 0)
        {
            options.Scenario = "horde";
        }
        if (options.BenchSeconds > 0)
        {
            options.Vsync = false;
        }
        return options;
    }
}
