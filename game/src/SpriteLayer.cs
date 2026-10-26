using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Анимированные спрайты одного листа одним MultiMesh (как <see cref="InstanceLayer"/>, ADR 0002): на экземпляр —
/// трансформ, цвет и custom data (столбец, ряд, вспышка), UV ячейки считает шейдер. Экземпляры сортируются по y
/// (ниже на экране — поверх), опора привязана к пикселю сетки листа: тексель ложится ровно в пиксель.
/// </summary>
public sealed class SpriteLayer
{
    private const int Stride = 16;

    private const string ShaderCode = @"
shader_type canvas_item;
uniform vec2 cell_uv;
uniform vec3 flash_color : source_color = vec3(1.0);
uniform bool mask_mode = false;
uniform vec4 mask_color : source_color = vec4(1.0);
varying float flash;
varying float corpse;

void vertex() {
    UV = (INSTANCE_CUSTOM.xy + UV) * cell_uv;
    flash = INSTANCE_CUSTOM.z;
    corpse = INSTANCE_CUSTOM.w;
}

void fragment() {
    vec4 texel = texture(TEXTURE, UV);
    COLOR = texel * COLOR;
    COLOR.rgb = mix(COLOR.rgb, flash_color, flash);
    if (mask_mode) {
        COLOR = vec4(mask_color.rgb, texel.a > 0.5 && corpse < 0.5 ? 1.0 : 0.0);
    }
}";

    /// <summary>Трупы рисуются под живыми: ключ сортировки уходит далеко вниз.</summary>
    private const float CorpseDepth = -1e6f;

    private static Shader? _shader;

    private readonly SpriteSheet _sheet;
    private readonly MultiMesh _multimesh;
    private readonly ShaderMaterial _material;
    private readonly float _texel;
    private Item[] _items;
    private float[] _keys;
    private int[] _order;
    private float[] _buffer = Array.Empty<float>();
    private int _capacity;
    private int _count;

    public SpriteLayer(SpriteSheet sheet, int initialCapacity)
    {
        _sheet = sheet;
        _texel = 1f / sheet.GridScale;
        _shader ??= new Shader { Code = ShaderCode };
        _material = new ShaderMaterial { Shader = _shader };
        Vector2 size = sheet.Texture.GetSize();
        _material.SetShaderParameter("cell_uv", new Vector2(sheet.CellWidth / size.X, sheet.CellHeight / size.Y));
        _material.SetShaderParameter("flash_color", MasterPalette.Instance.Role("flash"));
        _multimesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
            UseColors = true,
            UseCustomData = true,
            Mesh = ProceduralArt.UnitQuad(),
            CustomAabb = ProceduralArt.ArenaBounds(),
        };
        _items = new Item[initialCapacity];
        _keys = new float[initialCapacity];
        _order = new int[initialCapacity];
        Allocate(initialCapacity);
        Node = new MultiMeshInstance2D { Multimesh = _multimesh, Texture = sheet.Texture, Material = _material };
    }

    public MultiMeshInstance2D Node { get; }

    public SpriteSheet Sheet => _sheet;

    public int Count => _count;

    /// <summary>Режим маски для проверки читаемости: силуэты — одним плоским цветом.</summary>
    public void SetMask(bool on, Color color)
    {
        _material.SetShaderParameter("mask_mode", on);
        _material.SetShaderParameter("mask_color", color);
    }

    public void Begin() => _count = 0;

    /// <summary>
    /// Кадр (столбец, ряд) с опорой в точке мира (x, y); corpse — труп: под живыми и не входит в маску читаемости.
    /// </summary>
    public void Add(float x, float y, int column, int row, bool flip, Color modulate, float flash = 0f, bool corpse = false)
    {
        if (_count == _items.Length)
        {
            Array.Resize(ref _items, _items.Length * 2);
            Array.Resize(ref _keys, _items.Length);
            Array.Resize(ref _order, _items.Length);
        }
        _items[_count] = new Item { X = x, Y = y, Column = column, Row = row, Flip = flip, Modulate = modulate, Flash = flash, Corpse = corpse };
        _keys[_count] = corpse ? y + CorpseDepth : y;
        _order[_count] = _count;
        _count++;
    }

    public void End()
    {
        if (_count > _capacity)
        {
            int capacity = _capacity;
            while (capacity < _count)
            {
                capacity *= 2;
            }
            Allocate(capacity);
        }
        Array.Sort(_keys, _order, 0, _count);
        float grid = _sheet.GridScale;
        float[] b = _buffer;
        for (int k = 0; k < _count; k++)
        {
            ref Item it = ref _items[_order[k]];
            float texel = _texel;
            float w = _sheet.CellWidth * texel;
            float h = _sheet.CellHeight * texel;
            float sx = it.Flip ? -1f : 1f;
            float px = MathF.Round(it.X * grid) / grid;
            float py = MathF.Round(it.Y * grid) / grid;
            int o = k * Stride;
            b[o] = sx * w;
            b[o + 1] = 0;
            b[o + 2] = 0;
            b[o + 3] = px + sx * (w / 2 - _sheet.AnchorX * texel);
            b[o + 4] = 0;
            b[o + 5] = h;
            b[o + 6] = 0;
            b[o + 7] = py + h / 2 - _sheet.AnchorY * texel;
            b[o + 8] = it.Modulate.R;
            b[o + 9] = it.Modulate.G;
            b[o + 10] = it.Modulate.B;
            b[o + 11] = it.Modulate.A;
            b[o + 12] = it.Column;
            b[o + 13] = it.Row;
            b[o + 14] = it.Flash;
            b[o + 15] = it.Corpse ? 1f : 0f;
        }
        RenderingServer.MultimeshSetBuffer(_multimesh.GetRid(), _buffer);
        _multimesh.VisibleInstanceCount = _count;
    }

    private void Allocate(int capacity)
    {
        _capacity = capacity;
        _multimesh.InstanceCount = capacity;
        _buffer = new float[capacity * Stride];
    }

    private struct Item
    {
        public float X;
        public float Y;
        public int Column;
        public int Row;
        public bool Flip;
        public Color Modulate;
        public float Flash;
        public bool Corpse;
    }
}
