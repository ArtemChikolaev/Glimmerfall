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
    /// Пол в мировых тонах палитры (план светлоты: мир в нижней трети): мощение вперевязку 32 px, плиты разного тона,
    /// трещины, мох у швов, пятна земли и крошка. Тайл 512 px — повтор раз в 16 плит; рисунок от сида забега.
    /// </summary>
    public static Texture2D GroundTile(ulong seed)
    {
        const int size = 512;
        const int slab = 32;
        MasterPalette palette = MasterPalette.Instance;
        Color[] tones = { palette["stone2"], palette["stone2"], palette["stone2"], palette["stone1"], palette["stone3"] };
        Color seam = palette["stone1"];
        Color crack = palette["gloom2"];
        Color[] moss = { palette["moss1"], palette["moss2"] };
        Color[] dirt = { palette["earth1"], palette["earth2"] };
        Color grit = palette["stone3"];
        var random = new Random(unchecked((int)seed ^ (int)(seed >> 32)));
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int row = 0; row < size / slab; row++)
        {
            int offset = row % 2 == 0 ? 0 : slab / 2;
            for (int column = -1; column < size / slab; column++)
            {
                int x0 = column * slab + offset;
                int y0 = row * slab;
                Color tone = tones[random.Next(tones.Length)];
                bool mossy = random.NextDouble() < 0.12;
                bool dirty = random.NextDouble() < 0.08;
                for (int y = 0; y < slab; y++)
                {
                    for (int x = 0; x < slab; x++)
                    {
                        int px = ((x0 + x) % size + size) % size;
                        int py = y0 + y;
                        bool edge = x == 0 || y == 0;
                        Color c = edge ? seam : tone;
                        if (!edge && random.NextDouble() < 0.05)
                        {
                            c = grit;
                        }
                        bool nearSeam = x < 3 || y < 3 || x > slab - 3 || y > slab - 3;
                        if (!edge && mossy && nearSeam && random.NextDouble() < 0.55)
                        {
                            c = moss[random.Next(2)];
                        }
                        if (!edge && dirty && random.NextDouble() < 0.35)
                        {
                            c = dirt[random.Next(2)];
                        }
                        image.SetPixel(px, py, c);
                    }
                }
                if (random.NextDouble() < 0.22)
                {
                    int cx = x0 + 4 + random.Next(slab - 8);
                    int cy = y0 + 4 + random.Next(slab - 8);
                    for (int step = 0; step < 14; step++)
                    {
                        image.SetPixel(((cx % size) + size) % size, Math.Clamp(cy, 0, size - 1), crack);
                        cx += random.Next(3) - 1;
                        cy += random.Next(2) == 0 ? 1 : random.Next(3) - 1;
                    }
                }
            }
        }
        return ImageTexture.CreateFromImage(image);
    }

    /// <summary>Мягкое кольцо (белое, тонируется цветом экземпляра): вспышки смерти, уровня, удара босса.</summary>
    public static Texture2D Ring(int size)
    {
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float c = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Math.Clamp(1f - MathF.Abs(d - 0.86f) / 0.12f, 0f, 1f);
                image.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        }
        return ImageTexture.CreateFromImage(image);
    }
}
