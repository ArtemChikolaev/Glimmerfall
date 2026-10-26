using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Тач-управление (PRD §11, «под телефон с первого дня»): плавающий стик в левой половине экрана, кнопки рывка
/// и смены героя справа, «АВТО» — автопилот. Выбор в модалке — тапом по карточке. Всё сводится к тому же InputFrame.
/// Координаты — интерфейса в родном разрешении (база 1280×720).
/// </summary>
public partial class TouchControls : Control
{
    private const float StickRadius = 72f;
    private const float ButtonRadius = 44f;

    private int _stickFinger = -1;
    private int _dashFinger = -1;
    private int _swapFinger = -1;
    private Vector2 _origin;
    private Vector2 _current;
    private bool _autopilotToggle;

    public bool Used { get; private set; }
    public bool DashHeld => _dashFinger >= 0;
    public bool SwapHeld => _swapFinger >= 0;

    /// <summary>Отклонение стика в единичном круге; ноль, если палец не на стике.</summary>
    public Vector2 Stick
    {
        get
        {
            if (_stickFinger < 0)
            {
                return Vector2.Zero;
            }
            Vector2 v = (_current - _origin) / StickRadius;
            return v.LengthSquared() > 1f ? v.Normalized() : v;
        }
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public bool TakeAutopilotToggle()
    {
        bool toggle = _autopilotToggle;
        _autopilotToggle = false;
        return toggle;
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree())
        {
            return;
        }
        if (@event is InputEventScreenTouch touch)
        {
            Used = true;
            if (touch.Pressed)
            {
                Press(touch.Index, ToLocal(touch.Position));
            }
            else
            {
                Release(touch.Index);
            }
            QueueRedraw();
        }
        else if (@event is InputEventScreenDrag drag && drag.Index == _stickFinger)
        {
            _current = ToLocal(drag.Position);
            QueueRedraw();
        }
    }

    /// <summary>Касание приходит в пикселях окна; контрол живёт в координатах интерфейса.</summary>
    private Vector2 ToLocal(Vector2 windowPosition) => GetGlobalTransformWithCanvas().AffineInverse() * windowPosition;

    private void Press(int finger, Vector2 at)
    {
        Vector2 size = Size;
        if (AutoButton(size).HasPoint(at))
        {
            _autopilotToggle = true;
            return;
        }
        if (at.DistanceTo(DashCenter(size)) <= ButtonRadius * 1.3f)
        {
            _dashFinger = finger;
        }
        else if (at.DistanceTo(SwapCenter(size)) <= ButtonRadius * 1.3f)
        {
            _swapFinger = finger;
        }
        else if (at.X < size.X * 0.5f && _stickFinger < 0)
        {
            _stickFinger = finger;
            _origin = at;
            _current = at;
        }
    }

    private void Release(int finger)
    {
        if (finger == _stickFinger)
        {
            _stickFinger = -1;
        }
        if (finger == _dashFinger)
        {
            _dashFinger = -1;
        }
        if (finger == _swapFinger)
        {
            _swapFinger = -1;
        }
    }

    public override void _Draw()
    {
        if (!Used)
        {
            return;
        }
        Vector2 size = Size;
        Color ink = UiKit.WithAlpha(UiKit.Wax, 0.45f);
        Color idle = UiKit.WithAlpha(MasterPalette.Instance["ink"], 0.35f);
        Rect2 auto = AutoButton(size);
        DrawRect(auto, idle);
        DrawString(ThemeDB.FallbackFont, auto.Position + new Vector2(10, 24), "АВТО", HorizontalAlignment.Left, -1, 16, ink);
        DrawCircle(DashCenter(size), ButtonRadius, DashHeld ? ink : idle);
        DrawString(ThemeDB.FallbackFont, DashCenter(size) + new Vector2(-30, 6), "РЫВОК", HorizontalAlignment.Left, -1, 16, ink);
        DrawCircle(SwapCenter(size), ButtonRadius * 0.8f, SwapHeld ? ink : idle);
        DrawString(ThemeDB.FallbackFont, SwapCenter(size) + new Vector2(-28, 6), "СМЕНА", HorizontalAlignment.Left, -1, 16, ink);
        if (_stickFinger >= 0)
        {
            DrawArc(_origin, StickRadius, 0, Mathf.Tau, 32, ink, 3f);
            DrawCircle(_origin + Stick * StickRadius, 20f, ink);
        }
    }

    private static Vector2 DashCenter(Vector2 size) => new(size.X - 80f, size.Y - 110f);
    private static Vector2 SwapCenter(Vector2 size) => new(size.X - 192f, size.Y - 134f);
    private static Rect2 AutoButton(Vector2 size) => new(size.X - 96f, 80f, 80f, 36f);
}
