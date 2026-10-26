using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Тач-управление спайка (PRD §11, «под телефон с первого дня»): плавающий стик в левой половине экрана, кнопки рывка
/// и свапа справа, «AUTO» — автопилот, выбор в модалке — тапом по варианту. Всё сводится к тому же InputFrame.
/// </summary>
public partial class TouchControls : Control
{
    private const float StickRadius = 36f;
    private const float ButtonRadius = 22f;

    private int _stickFinger = -1;
    private int _dashFinger = -1;
    private int _swapFinger = -1;
    private Vector2 _origin;
    private Vector2 _current;
    private int _pendingPick = -1;
    private bool _autopilotToggle;
    private int _modalOptions;

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
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    /// <summary>Сколько вариантов показать в модалке (0 — модалки нет).</summary>
    public void SetModal(int options)
    {
        if (_modalOptions != options)
        {
            _modalOptions = options;
            QueueRedraw();
        }
    }

    /// <summary>Выбор в модалке, сделанный тапом с прошлого опроса; −1 — нет.</summary>
    public int TakePick()
    {
        int pick = _pendingPick;
        _pendingPick = -1;
        return pick;
    }

    public bool TakeAutopilotToggle()
    {
        bool toggle = _autopilotToggle;
        _autopilotToggle = false;
        return toggle;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            Used = true;
            if (touch.Pressed)
            {
                Press(touch.Index, touch.Position);
            }
            else
            {
                Release(touch.Index);
            }
            QueueRedraw();
        }
        else if (@event is InputEventScreenDrag drag && drag.Index == _stickFinger)
        {
            _current = drag.Position;
            QueueRedraw();
        }
    }

    private void Press(int finger, Vector2 at)
    {
        Vector2 size = GetViewportRect().Size;
        if (AutoButton(size).HasPoint(at))
        {
            _autopilotToggle = true;
            return;
        }
        if (_modalOptions > 0)
        {
            int option = (int)(at.X / size.X * _modalOptions);
            _pendingPick = Mathf.Clamp(option, 0, _modalOptions - 1);
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
        Vector2 size = GetViewportRect().Size;
        var ink = new Color(1f, 0.9f, 0.7f, 0.35f);
        Rect2 auto = AutoButton(size);
        DrawRect(auto, new Color(0f, 0f, 0f, 0.35f));
        DrawString(ThemeDB.FallbackFont, auto.Position + new Vector2(4, 12), "AUTO", HorizontalAlignment.Left, -1, 8, ink);
        if (_modalOptions > 0)
        {
            for (int k = 0; k < _modalOptions; k++)
            {
                float w = size.X / _modalOptions;
                DrawRect(new Rect2(k * w + 4, size.Y * 0.35f, w - 8, size.Y * 0.3f), new Color(0.1f, 0.08f, 0.12f, 0.55f));
                DrawString(ThemeDB.FallbackFont, new Vector2(k * w + 12, size.Y * 0.5f), $"[{k + 1}]", HorizontalAlignment.Left, -1, 12, ink);
            }
            return;
        }
        DrawCircle(DashCenter(size), ButtonRadius, DashHeld ? ink : new Color(0f, 0f, 0f, 0.3f));
        DrawString(ThemeDB.FallbackFont, DashCenter(size) + new Vector2(-12, 3), "DASH", HorizontalAlignment.Left, -1, 8, ink);
        DrawCircle(SwapCenter(size), ButtonRadius * 0.8f, SwapHeld ? ink : new Color(0f, 0f, 0f, 0.3f));
        DrawString(ThemeDB.FallbackFont, SwapCenter(size) + new Vector2(-12, 3), "SWAP", HorizontalAlignment.Left, -1, 8, ink);
        if (_stickFinger >= 0)
        {
            DrawArc(_origin, StickRadius, 0, Mathf.Tau, 32, ink, 1.5f);
            DrawCircle(_origin + Stick * StickRadius, 10f, ink);
        }
    }

    private static Vector2 DashCenter(Vector2 size) => new(size.X - 40f, size.Y - 40f);
    private static Vector2 SwapCenter(Vector2 size) => new(size.X - 96f, size.Y - 52f);
    private static Rect2 AutoButton(Vector2 size) => new(size.X - 44f, 4f, 40f, 16f);
}
