using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Настройки игрока в user://settings.cfg. Сложность и длина забега идут в конфиг сима (тир лестницы и число актов) —
/// поэтому они часть реплея; громкость, экран, тряска, FPS и облики героев — только презентация.
/// </summary>
public sealed class GameSettings
{
    private const string Path = "user://settings.cfg";

    public static readonly string[] DifficultyNames = { "Лёгкая", "Обычная", "Трудная" };
    public static readonly int[] RunLengths = { 1, 3 };

    /// <summary>Тир лестницы сложности (SimConfig.Tier): 0 — базовый.</summary>
    public int Difficulty;

    /// <summary>Число актов забега (по ~8 минут, босс в конце каждого).</summary>
    public int Acts = 1;

    public float MasterVolume = 0.8f;
    public float EffectsVolume = 0.8f;
    public bool Fullscreen;
    public bool ScreenShake = true;
    public bool ShowFps;

    /// <summary>Выбранный облик каждого героя (лист из <see cref="GameText.Skins"/>) — косметика, не в конфиге сима.</summary>
    public string[] Skins = GameText.DefaultSkins();

    public static GameSettings Load()
    {
        var settings = new GameSettings();
        var file = new ConfigFile();
        if (file.Load(Path) != Error.Ok)
        {
            return settings;
        }
        settings.Difficulty = Math.Clamp((int)file.GetValue("run", "difficulty", 0), 0, DifficultyNames.Length - 1);
        int acts = (int)file.GetValue("run", "acts", 1);
        settings.Acts = Array.IndexOf(RunLengths, acts) >= 0 ? acts : 1;
        settings.MasterVolume = Math.Clamp((float)file.GetValue("audio", "master", 0.8f), 0f, 1f);
        settings.EffectsVolume = Math.Clamp((float)file.GetValue("audio", "effects", 0.8f), 0f, 1f);
        settings.Fullscreen = (bool)file.GetValue("video", "fullscreen", false);
        settings.ScreenShake = (bool)file.GetValue("video", "shake", true);
        settings.ShowFps = (bool)file.GetValue("video", "fps", false);
        for (int hero = 0; hero < settings.Skins.Length; hero++)
        {
            string sheet = (string)file.GetValue("cosmetics", $"hero{hero}", settings.Skins[hero]);
            // облик из старой версии или с другой машины, которого нет в каталоге, — базовый
            if (Array.Exists(GameText.Skins[hero], skin => skin.Sheet == sheet))
            {
                settings.Skins[hero] = sheet;
            }
        }
        return settings;
    }

    public void Save()
    {
        var file = new ConfigFile();
        file.SetValue("run", "difficulty", Difficulty);
        file.SetValue("run", "acts", Acts);
        file.SetValue("audio", "master", MasterVolume);
        file.SetValue("audio", "effects", EffectsVolume);
        file.SetValue("video", "fullscreen", Fullscreen);
        file.SetValue("video", "shake", ScreenShake);
        file.SetValue("video", "fps", ShowFps);
        for (int hero = 0; hero < Skins.Length; hero++)
        {
            file.SetValue("cosmetics", $"hero{hero}", Skins[hero]);
        }
        file.Save(Path);
    }

    /// <summary>Применяет то, что не требует перезапуска забега: окно и громкость.</summary>
    public void Apply()
    {
        DisplayServer.WindowMode mode = Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
        if (DisplayServer.GetName() != "headless" && DisplayServer.WindowGetMode() != mode)
        {
            DisplayServer.WindowSetMode(mode);
        }
        Sfx.SetVolumes(MasterVolume, EffectsVolume);
    }
}
