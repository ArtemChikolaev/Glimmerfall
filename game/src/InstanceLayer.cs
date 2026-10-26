using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Слой экземпляров одного вида: один MultiMeshInstance2D, буфер float на кадр и один вызов
/// <see cref="RenderingServer.MultimeshSetBuffer(Rid, ReadOnlySpan{float})"/> — без нод на сущность (ADR 0002).
/// Раскладка 2D: (x.x, y.x, pad, origin.x, x.y, y.y, pad, origin.y) + RGBA; ёмкость растёт удвоением.
/// </summary>
public sealed class InstanceLayer
{
    private const int Stride = 12;

    private readonly MultiMesh _multimesh;
    private float[] _buffer = Array.Empty<float>();
    private int _capacity;
    private int _count;

    public InstanceLayer(Texture2D texture, Material? material, int initialCapacity)
    {
        _multimesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
            UseColors = true,
            Mesh = ProceduralArt.UnitQuad(),
        };
        Allocate(initialCapacity);
        Node = new MultiMeshInstance2D { Multimesh = _multimesh, Texture = texture, Material = material };
    }

    public MultiMeshInstance2D Node { get; }

    public int Count => _count;

    public void Begin() => _count = 0;

    public void Add(float x, float y, float size, Color color)
    {
        if (_count == _capacity)
        {
            Grow();
        }
        int o = _count * Stride;
        float[] b = _buffer;
        b[o] = size;
        b[o + 1] = 0;
        b[o + 2] = 0;
        b[o + 3] = x;
        b[o + 4] = 0;
        b[o + 5] = size;
        b[o + 6] = 0;
        b[o + 7] = y;
        b[o + 8] = color.R;
        b[o + 9] = color.G;
        b[o + 10] = color.B;
        b[o + 11] = color.A;
        _count++;
    }

    public void End()
    {
        RenderingServer.MultimeshSetBuffer(_multimesh.GetRid(), _buffer);
        _multimesh.VisibleInstanceCount = _count;
    }

    private void Allocate(int capacity)
    {
        _capacity = capacity;
        _multimesh.InstanceCount = capacity;
        _buffer = new float[capacity * Stride];
    }

    private void Grow()
    {
        float[] old = _buffer;
        Allocate(_capacity * 2);
        Array.Copy(old, _buffer, old.Length);
    }
}
