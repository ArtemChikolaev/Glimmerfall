using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Спрайт-лист конвейера tools/sprites: метаданные из content/art/sprites (вшиты в сборку), картинка — res://art/sprites.
/// Ряд на пару (клип, направление), столбец на кадр; зеркальные направления отражаются при отрисовке (5 + зеркало).
/// </summary>
public sealed class SpriteSheet
{
    /// <summary>Восемь направлений по экрану, по часовой от «вниз»: индекс = угол / 45°.</summary>
    public static readonly string[] Directions = { "S", "SE", "E", "NE", "N", "NW", "W", "SW" };

    private readonly Dictionary<string, SpriteClip> _clips = new();

    private SpriteSheet(string id)
    {
        Id = id;
        using JsonDocument document = ContentFiles.Open($"sprites/{id}.json");
        JsonElement root = document.RootElement;
        JsonElement cell = root.GetProperty("cell");
        CellWidth = cell[0].GetInt32();
        CellHeight = cell[1].GetInt32();
        JsonElement anchor = root.GetProperty("anchor");
        AnchorX = anchor[0].GetInt32();
        AnchorY = anchor[1].GetInt32();
        GridScale = root.GetProperty("grid")[0].GetInt32() / 640f;
        TargetPx = root.GetProperty("source").TryGetProperty("target_px", out JsonElement target) ? target.GetSingle() : 48f;
        string image = root.GetProperty("image").GetString()!;
        Texture = GD.Load<Texture2D>($"res://art/sprites/{image}")
            ?? throw new InvalidOperationException($"лист {image} не импортирован: make import");
        var mirror = new Dictionary<string, string>();
        foreach (JsonProperty m in root.GetProperty("directions").GetProperty("mirror").EnumerateObject())
        {
            mirror[m.Name] = m.Value.GetString()!;
        }
        foreach (JsonProperty clip in root.GetProperty("clips").EnumerateObject())
        {
            _clips[clip.Name] = new SpriteClip(clip.Name, clip.Value, mirror);
        }
    }

    public string Id { get; }
    public Texture2D Texture { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }
    public int AnchorX { get; }
    public int AnchorY { get; }

    /// <summary>Во сколько раз сетка листа крупнее базовой 640×360 (1 или 1,5).</summary>
    public float GridScale { get; }

    /// <summary>Рост силуэта в стойке на сетке 640×360 (ADR 0003 п. 3): портрет и надписи над фигурой — от него.</summary>
    public float TargetPx { get; }

    public SpriteClip this[string clip] => _clips[clip];

    public bool Has(string clip) => _clips.ContainsKey(clip);

    public static SpriteSheet Load(string id) => new(id);

    /// <summary>Направление по вектору на экране (y вниз): 0 — S, 2 — E, 4 — N, 6 — W.</summary>
    public static int Direction(float dx, float dy)
    {
        int sector = (int)MathF.Round(MathF.Atan2(dx, dy) / (MathF.PI / 4));
        return ((sector % 8) + 8) % 8;
    }
}

public sealed class SpriteClip
{
    private readonly int[] _row = new int[8];
    private readonly bool[] _flip = new bool[8];

    public SpriteClip(string name, JsonElement json, IReadOnlyDictionary<string, string> mirror)
    {
        Name = name;
        Frames = json.GetProperty("frames").GetInt32();
        Fps = json.GetProperty("fps").GetSingle();
        Loop = json.GetProperty("loop").GetBoolean();
        WindupEnd = ActiveEnd = -1;
        if (json.TryGetProperty("phases", out JsonElement phases))
        {
            WindupEnd = phases.GetProperty("windup")[1].GetInt32();
            ActiveEnd = phases.GetProperty("active")[1].GetInt32();
        }
        int row = json.GetProperty("row").GetInt32();
        var candidates = new List<(int Dir, int Row, bool Flip)>();
        int k = 0;
        foreach (JsonElement dir in json.GetProperty("dirs").EnumerateArray())
        {
            candidates.Add((Array.IndexOf(SpriteSheet.Directions, dir.GetString()), row + k++, false));
        }
        foreach ((string target, string source) in mirror)
        {
            int found = candidates.FindIndex(c => !c.Flip && SpriteSheet.Directions[c.Dir] == source);
            if (found >= 0)
            {
                candidates.Add((Array.IndexOf(SpriteSheet.Directions, target), candidates[found].Row, true));
            }
        }
        // У клипа может не быть части направлений (смерть — одно, рядовые — 4): берём ближайшее по углу,
        // при равенстве — то, что смотрит в ту же сторону по горизонтали (диагональ бега читается как бок).
        for (int target = 0; target < 8; target++)
        {
            (int Dir, int Row, bool Flip) best = candidates[0];
            int bestScore = int.MaxValue;
            foreach ((int Dir, int Row, bool Flip) c in candidates)
            {
                int distance = Math.Abs(target - c.Dir) % 8;
                distance = Math.Min(distance, 8 - distance);
                int score = distance * 2 + (Side(c.Dir) == Side(target) ? 0 : 1);
                if (score < bestScore)
                {
                    best = c;
                    bestScore = score;
                }
            }
            _row[target] = best.Row;
            _flip[target] = best.Flip;
        }
    }

    public string Name { get; }
    public int Frames { get; }
    public float Fps { get; }
    public bool Loop { get; }

    /// <summary>Конец замаха и конец активной фазы (кадры, не включая); −1 — у клипа нет фаз.</summary>
    public int WindupEnd { get; }
    public int ActiveEnd { get; }

    public float Duration => Frames / Fps;

    public int Row(int direction) => _row[direction];

    public bool Flip(int direction) => _flip[direction];

    public int FrameAt(float seconds)
    {
        int frame = (int)(Math.Max(seconds, 0f) * Fps);
        return Loop ? frame % Frames : Math.Min(frame, Frames - 1);
    }

    private static int Side(int direction) => direction is 1 or 2 or 3 ? 1 : direction is 5 or 6 or 7 ? -1 : 0;
}
