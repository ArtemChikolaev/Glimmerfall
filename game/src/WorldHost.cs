using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Пиксельный мир во внутреннем SubViewport на сетке 640×360 (ADR 0003 п. 2), показанный целым масштабом; интерфейс
/// живёт в окне в родном разрешении (п. 7). Поле расширяется под пропорции окна (16:10 → 640×400).
/// Внутри два холста: слой 0 — пол, декор и фигуры (на них действуют тьма и свет), слой 1 — эффекты и телеграфы
/// поверх, всегда яркие.
/// </summary>
public partial class WorldHost : Node
{
    private TextureRect _display = null!;
    private Vector2 _cameraCenter;
    private Vector2 _shake;

    public SubViewport Viewport { get; private set; } = null!;

    /// <summary>Корень слоя 0: сюда кладёт свои узлы <see cref="WorldView"/>.</summary>
    public Node2D World { get; private set; } = null!;

    /// <summary>Слой эффектов поверх мира, следует за камерой.</summary>
    public CanvasLayer Fx { get; private set; } = null!;

    public Camera2D Camera { get; private set; } = null!;
    public CanvasModulate Darkness { get; private set; } = null!;
    public float GridScale { get; private set; } = 1f;
    public int Factor { get; private set; } = 1;

    public static WorldHost Create(Node parent, float gridScale)
    {
        var host = new WorldHost { Name = "WorldHost", GridScale = gridScale };
        parent.AddChild(host);
        host.Build();
        return host;
    }

    private void Build()
    {
        Viewport = new SubViewport
        {
            Name = "WorldViewport",
            CanvasItemDefaultTextureFilter = Godot.Viewport.DefaultCanvasItemTextureFilter.Nearest,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            HandleInputLocally = false,
            Disable3D = true,
        };
        AddChild(Viewport);
        World = new Node2D { Name = "World" };
        Viewport.AddChild(World);
        Darkness = new CanvasModulate { Color = Colors.White };
        Viewport.AddChild(Darkness);
        Fx = new CanvasLayer { Name = "Fx", Layer = 1, FollowViewportEnabled = true };
        Viewport.AddChild(Fx);
        Camera = new Camera2D { Zoom = new Vector2(GridScale, GridScale) };
        Viewport.AddChild(Camera);
        Camera.MakeCurrent();

        var screen = new CanvasLayer { Name = "WorldScreen", Layer = -1 };
        AddChild(screen);
        _display = new TextureRect
        {
            Texture = Viewport.GetTexture(),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        screen.AddChild(_display);
        GetTree().Root.SizeChanged += Layout;
        Layout();
    }

    public override void _ExitTree()
    {
        GetTree().Root.SizeChanged -= Layout;
    }

    /// <summary>Целый масштаб по окну; поле — окно, делённое на масштаб (с округлением вверх, лишнее срезается по краям).</summary>
    public void Layout()
    {
        Window root = GetTree().Root;
        Vector2I window = root.Size;
        int baseW = Mathf.RoundToInt(640 * GridScale);
        int baseH = Mathf.RoundToInt(360 * GridScale);
        Factor = Math.Max(1, Math.Min(window.X / baseW, window.Y / baseH));
        var size = new Vector2I((window.X + Factor - 1) / Factor, (window.Y + Factor - 1) / Factor);
        Viewport.Size = size;
        Vector2 ui = root.GetVisibleRect().Size;
        float unitsPerPixel = ui.X / Math.Max(window.X, 1);
        _display.Size = (Vector2)(size * Factor) * unitsPerPixel;
        _display.Position = (ui - _display.Size) / 2;
    }

    /// <summary>Центр кадра в мире; позиция камеры привязана к пикселю сетки, тряска — целыми пикселями.</summary>
    public void LookAt(Vector2 center, Vector2 shakePixels)
    {
        _cameraCenter = center;
        _shake = shakePixels.Round() / GridScale;
        Camera.Position = (center * GridScale).Round() / GridScale + _shake;
    }

    /// <summary>Точка мира → координаты интерфейса (для цифр урона и подписей над фигурами).</summary>
    public Vector2 WorldToUi(Vector2 world)
    {
        Vector2 viewportPixel = (world - Camera.Position) * GridScale + (Vector2)Viewport.Size / 2;
        float scale = _display.Size.X / Math.Max(Viewport.Size.X, 1);
        return _display.Position + viewportPixel * scale;
    }

    /// <summary>Полуразмер видимого поля в мировых единицах — для отсечения.</summary>
    public Vector2 HalfView => (Vector2)Viewport.Size / (2 * GridScale);

    public Image CaptureWorld() => Viewport.GetTexture().GetImage();
}
