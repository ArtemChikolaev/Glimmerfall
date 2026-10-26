using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Заглушки спайка, нарисованные кодом: ни одного внешнего ассета — нечего вносить в реестр происхождения.
/// Цвета — черновик «готической светотени» (ADR 0003), не стиль-библия.
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

    public static readonly Color[] ProjectileColors =
    {
        new(1.00f, 0.55f, 0.15f),
        new(0.50f, 0.82f, 1.00f),
        new(0.80f, 0.72f, 1.00f),
    };

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

    /// <summary>Тело врага: светлый круг с тёмным контуром и бликом — тонируется цветом экземпляра.</summary>
    public static Texture2D Body(int size)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float c = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                if (d > 1f)
                {
                    image.SetPixel(x, y, Colors.Transparent);
                    continue;
                }
                float light = 0.75f + 0.25f * Math.Clamp((c - y) / c - (x - c) / (2 * c), -1f, 1f);
                Color color = d > 0.78f ? new Color(0.08f, 0.05f, 0.07f) : new Color(light, light, light);
                image.SetPixel(x, y, color);
            }
        }
        return ImageTexture.CreateFromImage(image);
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

    /// <summary>Тайл пола: тёмные плиты со швами и шумом — чтобы движение камеры читалось.</summary>
    public static Texture2D GroundTile(int size)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var random = new Random(7);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool seam = x % (size / 2) == 0 || y % (size / 2) == 0;
                float n = (float)random.NextDouble() * 0.025f;
                float v = seam ? 0.045f : 0.085f + n;
                image.SetPixel(x, y, new Color(v * 0.95f, v * 0.9f, v * 1.1f));
            }
        }
        return ImageTexture.CreateFromImage(image);
    }
}
