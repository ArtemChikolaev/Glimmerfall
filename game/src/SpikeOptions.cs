using System;
using System.Globalization;

namespace Glimmerfall.Game;

/// <summary>
/// Пользовательские аргументы после «--»: <c>Glimmerfall.x86_64 -- --bench-seconds 60</c>.
/// По умолчанию — спайк орды с автопилотом-кайтером; <c>--scenario run --manual</c> — играбельный акт.
/// </summary>
public sealed class SpikeOptions
{
    public string Scenario = "horde";
    public int Enemies = 5000;
    public int Projectiles = 2000;
    public int Acts = 1;
    public string Seed = "spike";
    public bool Autopilot = true;
    public int BenchSeconds;
    public int WarmupSeconds = 5;
    public int SmokeFrames;
    public bool Vsync = true;

    /// <summary>Замер закончился — выйти (ПК, CI) или оставить отчёт на экране для скриншота (телефон, Deck в игровом режиме).</summary>
    public bool QuitAfterBench = true;

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
                default: throw new ArgumentException($"unknown option {key}");
            }
        }
        if (options.BenchSeconds > 0)
        {
            options.Vsync = false;
        }
        return options;
    }
}
