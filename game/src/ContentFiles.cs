using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Данные из content/ вшиты в сборку игры (EmbeddedResource в Glimmerfall.csproj): один источник правды на диске,
/// одинаково в редакторе и в любом экспорте, включая NativeAOT. Разбор — JsonDocument, без рефлексии.
/// </summary>
public static class ContentFiles
{
    public static JsonDocument Open(string logicalName)
    {
        using Stream stream = typeof(ContentFiles).Assembly.GetManifestResourceStream(logicalName)
            ?? throw new InvalidOperationException($"нет вшитого файла content: {logicalName} (make sprites / make palette)");
        return JsonDocument.Parse(stream);
    }
}

/// <summary>Мастер-палитра (content/art/palette.json): цвета по id и роли — презентация не заводит своих цветов угроз.</summary>
public sealed class MasterPalette
{
    private readonly Dictionary<string, Color> _colors = new();
    private readonly Dictionary<string, string> _roles = new();

    private MasterPalette()
    {
        using JsonDocument document = ContentFiles.Open("palette.json");
        foreach (JsonProperty color in document.RootElement.GetProperty("colors").EnumerateObject())
        {
            _colors[color.Name] = Color.FromHtml(color.Value.GetString()!);
        }
        foreach (JsonProperty role in document.RootElement.GetProperty("roles").EnumerateObject())
        {
            _roles[role.Name] = role.Value.GetString()!;
        }
    }

    public static MasterPalette Instance { get; } = new();

    public Color this[string id] => _colors.TryGetValue(id, out Color color) ? color : throw new KeyNotFoundException($"нет цвета {id} в палитре");

    public Color Role(string role) => this[_roles[role]];
}
