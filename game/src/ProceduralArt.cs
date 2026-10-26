using System;
using Glimmerfall.Sim;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Заглушки спайка, нарисованные кодом: свечения, квад MultiMesh, пол из мировых тонов палитры. Тинты врагов
/// и героев — временные метки видов поверх листов манекенов, не стиль-библия (её цвета — в палитре).
/// </summary>
public static class ProceduralArt
{
    public static readonly Color[] EnemyColors =
    {
        new(0.55f, 0.20f, 0.22f), // shambler
        new(0.78f, 0.36f, 0.20f), // runner
        new(0.46f, 0.26f, 0.42f), // brute
        new(0.32f, 0.58f, 0.30f), // spitter
        new(0.74f, 0.58f, 0.20f), // charger
        new(0.90f, 0.16f, 0.48f), // ravager (элита)
        new(0.96f, 0.86f, 0.32f), // gloom warden (босс)
    };

    public static readonly Color[] HeroColors =
    {
        new(1.00f, 0.74f, 0.34f), // lamplighter — огонь
        new(0.62f, 0.82f, 1.00f), // warden — мороз
        new(0.78f, 0.66f, 1.00f), // sparkwright — молния
    };

    /// <summary>
    /// Границы отсечения слоёв MultiMesh: вся арена с запасом. Буфер заливается через RenderingServer, и Godot
    /// не пересчитывает AABB сам — без явных границ слой отсекается целиком, когда камера уходит от места первого кадра.
    /// </summary>
    public static Aabb ArenaBounds()
    {
        const float margin = 512f;
        return new Aabb(new Vector3(-margin, -margin, -1f), new Vector3(SimConstants.ArenaSize + 2 * margin, SimConstants.ArenaSize + 2 * margin, 2f));
    }

    /// <summary>Единичный квад для MultiMesh в 2D: вершины Vector2, UV по углам; масштаб задаёт трансформ экземпляра.</summary>
    public static ArrayMesh UnitQuad()
    {
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = new[] { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f) };
        arrays[(int)Mesh.ArrayType.TexUV] = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        arrays[(int)Mesh.ArrayType.Index] = new[] { 0, 1, 2, 0, 2, 3 };
        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }

    /// <summary>Мягкое свечение для снарядов, частиц и света фонаря (рисуется аддитивно).</summary>
    public static Texture2D Glow(int size, float falloff)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float c = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = MathF.Pow(Math.Clamp(1f - d, 0f, 1f), falloff);
                image.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        return ImageTexture.CreateFromImage(image);
    }

    /// <summary>
    /// Тайл пола в мировых тонах палитры (план светлоты: мир в нижней трети): плиты stone2 со швами stone1 и редкой
    /// крошкой stone3 — чтобы движение камеры читалось, а силуэты оставались светлее пола.
    /// </summary>
    public static Texture2D GroundTile(int size)
    {
        MasterPalette palette = MasterPalette.Instance;
        Color slab = palette["stone2"];
        Color seam = palette["stone1"];
        Color grit = palette["stone3"];
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var random = new Random(7);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isSeam = x % (size / 2) == 0 || y % (size / 2) == 0;
                bool isGrit = !isSeam && random.NextDouble() < 0.06;
                image.SetPixel(x, y, isSeam ? seam : isGrit ? grit : slab);
            }
        }
        return ImageTexture.CreateFromImage(image);
    }
}
