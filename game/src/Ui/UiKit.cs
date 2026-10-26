using System;
using Godot;

namespace Glimmerfall.Game;

/// <summary>
/// Общий вид интерфейса по стиль-библии (docs/art/style-bible.md, §5): копоть и воск, латунь фонарей, свет стекла.
/// Заголовки — системный шрифт с засечками (ассета нет — нечего лицензировать; на Linux — DejaVu/Liberation Serif),
/// тексты и числа — шрифт движка. Цвета — из мастер-палитры.
/// </summary>
public static class UiKit
{
    private static Theme? _theme;
    private static Font? _serif;

    public static Color Soot => WithAlpha(MasterPalette.Instance["gloom1"], 0.92f);
    public static Color SootLight => WithAlpha(MasterPalette.Instance["gloom2"], 0.94f);
    public static Color Brass => MasterPalette.Instance["brass2"];
    public static Color BrassDark => MasterPalette.Instance["brass1"];
    public static Color Wax => MasterPalette.Instance["bone4"];
    public static Color WaxDim => MasterPalette.Instance["bone3"];
    public static Color Glow => MasterPalette.Instance["radiance2"];
    public static Color Blood => MasterPalette.Instance["crimson3"];
    public static Color Ember => MasterPalette.Instance["fire3"];

    public static Font Serif => _serif ??= new SystemFont
    {
        FontNames = new[] { "Georgia", "Palatino", "Times New Roman", "DejaVu Serif", "Liberation Serif", "Noto Serif", "serif" },
        FontWeight = 600,
    };

    public static Color WithAlpha(Color color, float alpha) => new(color.R, color.G, color.B, alpha);

    public static Theme Theme => _theme ??= BuildTheme();

    private static Theme BuildTheme()
    {
        var theme = new Theme { DefaultFontSize = 18 };
        theme.SetColor("font_color", "Label", Wax);
        theme.SetColor("font_outline_color", "Label", MasterPalette.Instance["ink"]);
        theme.SetConstant("outline_size", "Label", 4);

        theme.SetStylebox("normal", "Button", Box(SootLight, BrassDark, 2));
        theme.SetStylebox("hover", "Button", Box(WithAlpha(MasterPalette.Instance["gloom3"], 0.96f), Brass, 2));
        theme.SetStylebox("pressed", "Button", Box(WithAlpha(MasterPalette.Instance["earth3"], 0.96f), Glow, 2));
        theme.SetStylebox("focus", "Button", Box(new Color(0, 0, 0, 0), Glow, 2));
        theme.SetStylebox("disabled", "Button", Box(Soot, MasterPalette.Instance["stone3"], 1));
        theme.SetColor("font_color", "Button", Wax);
        theme.SetColor("font_hover_color", "Button", Glow);
        theme.SetColor("font_pressed_color", "Button", Glow);
        theme.SetColor("font_focus_color", "Button", Glow);
        theme.SetFont("font", "Button", Serif);
        theme.SetFontSize("font_size", "Button", 22);

        theme.SetStylebox("panel", "PanelContainer", Box(Soot, BrassDark, 2, 10));
        theme.SetStylebox("panel", "Panel", Box(Soot, BrassDark, 2, 10));

        theme.SetColor("font_color", "CheckBox", Wax);
        theme.SetColor("font_hover_color", "CheckBox", Glow);
        theme.SetColor("font_pressed_color", "CheckBox", Wax);
        theme.SetColor("font_hover_pressed_color", "CheckBox", Glow);
        theme.SetColor("font_focus_color", "CheckBox", Glow);
        var plain = new StyleBoxEmpty { ContentMarginLeft = 2, ContentMarginTop = 4, ContentMarginBottom = 4 };
        foreach (string state in new[] { "normal", "hover", "pressed", "hover_pressed", "disabled" })
        {
            theme.SetStylebox(state, "CheckBox", plain);
        }
        theme.SetStylebox("focus", "CheckBox", Box(new Color(0, 0, 0, 0), Glow, 1, 2));
        theme.SetColor("font_color", "OptionButton", Wax);
        theme.SetStylebox("normal", "OptionButton", Box(SootLight, BrassDark, 2));
        theme.SetStylebox("hover", "OptionButton", Box(WithAlpha(MasterPalette.Instance["gloom3"], 0.96f), Brass, 2));
        theme.SetStylebox("focus", "OptionButton", Box(new Color(0, 0, 0, 0), Glow, 2));

        var slider = new StyleBoxFlat { BgColor = MasterPalette.Instance["gloom3"], ContentMarginTop = 4, ContentMarginBottom = 4 };
        slider.SetCornerRadiusAll(3);
        theme.SetStylebox("slider", "HSlider", slider);
        var fill = new StyleBoxFlat { BgColor = Brass, ContentMarginTop = 4, ContentMarginBottom = 4 };
        fill.SetCornerRadiusAll(3);
        theme.SetStylebox("grabber_area", "HSlider", fill);
        theme.SetStylebox("grabber_area_highlight", "HSlider", fill);
        return theme;
    }

    public static StyleBoxFlat Box(Color fill, Color border, int borderWidth, int padding = 6)
    {
        var box = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            ContentMarginLeft = padding * 2,
            ContentMarginRight = padding * 2,
            ContentMarginTop = padding,
            ContentMarginBottom = padding,
        };
        box.SetBorderWidthAll(borderWidth);
        box.SetCornerRadiusAll(4);
        return box;
    }

    public static Label Text(string text, int size = 18, Color? color = null, bool serif = false)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", size);
        if (color is Color c)
        {
            label.AddThemeColorOverride("font_color", c);
        }
        if (serif)
        {
            label.AddThemeFontOverride("font", Serif);
        }
        return label;
    }

    public static Button Button(string text, Action onPressed, Sfx? sfx, int width = 320)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(width, 48), FocusMode = Control.FocusModeEnum.All };
        button.Pressed += () =>
        {
            sfx?.Play(Sound.UiClick, 0.8f, 0f);
            onPressed();
        };
        button.MouseEntered += () => sfx?.Play(Sound.UiHover, 0.6f, 0f);
        button.FocusEntered += () => sfx?.Play(Sound.UiHover, 0.6f, 0f);
        return button;
    }

    /// <summary>Затемнение под модальной панелью: ловит мышь, чтобы клики не уходили в мир.</summary>
    public static ColorRect Scrim(float alpha)
    {
        var scrim = new ColorRect { Color = WithAlpha(MasterPalette.Instance["ink"], alpha), MouseFilter = Control.MouseFilterEnum.Stop };
        scrim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        return scrim;
    }

    public static Control Centered(Control child)
    {
        var center = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        center.AddChild(child);
        return center;
    }
}
