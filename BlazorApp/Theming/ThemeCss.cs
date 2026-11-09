using System.Globalization;
using System.Text;
using ApiContracts;
using BlazorApp.Services;

namespace BlazorApp.Theming;

// Turns a ProfileThemeDto into the CSS custom properties, background and classes that the
// profile container uses. The shell uses the same variable names (--bg, --surface, --accent ...),
// so every shared component (post cards, chips, buttons) is themed automatically inside a profile.
public static class ThemeCss
{
    // already loaded by App.razor
    private static readonly HashSet<string> shellFonts = new() { "Unbounded", "Space Grotesk", "JetBrains Mono" };

    private const string NoiseSvg =
        "url(\"data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='160' height='160'><filter id='n'><feTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='2' stitchTiles='stitch'/><feColorMatrix values='0 0 0 0 0.5 0 0 0 0 0.5 0 0 0 0 0.5 0 0 0 0.35 0'/></filter><rect width='160' height='160' filter='url(%23n)'/></svg>\")";

    public static string Variables(ProfileThemeDto theme)
    {
        string bg = CssValues.Color(theme.Background, "#0a0a0b");
        string surface = CssValues.Color(theme.Surface, "#121215");
        string text = CssValues.Color(theme.Text, "#ecebe6");
        string accent = CssValues.Color(theme.Accent, "#ff2a4f");

        StringBuilder css = new();
        css.Append($"--bg:{bg};");
        css.Append($"--surface:{surface};");
        css.Append($"--surface-2:color-mix(in srgb, {surface} 84%, {text});");
        css.Append($"--text:{text};");
        css.Append($"--muted:{CssValues.Color(theme.Muted, "#85858f")};");
        css.Append($"--accent:{accent};");
        css.Append($"--on-accent:{ContrastText(accent)};");
        css.Append($"--accent-2:{CssValues.Color(theme.Accent2, "#2f45ff")};");
        css.Append($"--line:{CssValues.Color(theme.Border, "#2a2a31")};");
        css.Append($"--radius:{CssValues.Clamp(theme.Radius, 0, 40)}px;");
        css.Append($"--border-w:{CssValues.Clamp(theme.BorderWidth, 0, 8)}px;");
        css.Append($"--glow-color:{CssValues.Color(theme.GlowColor, accent)};");
        css.Append($"--glow:{CssValues.Clamp(theme.GlowStrength, 0, 60)}px;");
        css.Append($"--font-display:'{CssValues.Font(theme.DisplayFont, "Unbounded")}', 'Unbounded', sans-serif;");
        css.Append($"--font-body:'{CssValues.Font(theme.BodyFont, "Space Grotesk")}', 'Space Grotesk', system-ui, sans-serif;");
        css.Append($"--font-mono:'{CssValues.Font(theme.MonoFont, "JetBrains Mono")}', 'JetBrains Mono', ui-monospace, monospace;");
        css.Append($"--base-size:{CssValues.Clamp(theme.BaseFontSize, 12, 22)}px;");
        css.Append($"--heading-case:{(theme.HeadingCase == "upper" ? "uppercase" : "none")};");
        css.Append($"--tracking:{(CssValues.Clamp(theme.LetterSpacing, -5, 20) / 100.0).ToString(CultureInfo.InvariantCulture)}em;");
        css.Append($"--gap:{CssValues.Clamp(theme.GridGap, 0, 48)}px;");
        css.Append($"--banner-h:{CssValues.Clamp(theme.BannerHeight, 0, 520)}px;");
        css.Append($"--avatar-border:{CssValues.Clamp(theme.AvatarBorder, 0, 10)}px;");
        css.Append($"--pattern-color:{CssValues.Color(theme.PatternColor, "#1c1c22")};");
        css.Append($"--fx:{(CssValues.Clamp(theme.EffectIntensity, 0, 100) / 100.0).ToString(CultureInfo.InvariantCulture)};");
        return css.ToString();
    }

    public static string Background(ProfileThemeDto theme, ApiConfig api)
    {
        string bg = CssValues.Color(theme.Background, "#0a0a0b");
        switch (theme.BackgroundMode)
        {
            case "gradient":
            {
                List<string> stops = new() { CssValues.Color(theme.GradientFrom, bg) };
                if (CssValues.IsColor(theme.GradientVia))
                {
                    stops.Add(theme.GradientVia.Trim());
                }

                stops.Add(CssValues.Color(theme.GradientTo, bg));
                int angle = CssValues.Clamp(theme.GradientAngle, 0, 360);
                return $"background-color:{bg};background-image:linear-gradient({angle}deg, {string.Join(", ", stops)});background-attachment:fixed;";
            }
            case "image" when CssValues.IsUrl(theme.BackgroundImageUrl):
            {
                string dim = (CssValues.Clamp(theme.BackgroundDim, 0, 95) / 100.0).ToString(CultureInfo.InvariantCulture);
                bool tile = theme.BackgroundImageFit == "tile";
                return $"background-color:{bg};" +
                       $"background-image:linear-gradient(rgba(0,0,0,{dim}), rgba(0,0,0,{dim})), url(\"{api.Resolve(theme.BackgroundImageUrl)}\");" +
                       $"background-size:{(tile ? "auto, auto" : "cover, cover")};" +
                       $"background-repeat:{(tile ? "repeat" : "no-repeat")};background-position:center;background-attachment:fixed;";
            }
            case "pattern":
                return $"background-color:{bg};{Pattern(theme.Pattern)}";
            default:
                return $"background-color:{bg};";
        }
    }

    // The theme's background for a small card (a post in the feed). Same as Background(),
    // but nothing is fixed to the screen, so every card shows its own copy.
    public static string CardBackground(ProfileThemeDto theme, ApiConfig api)
    {
        string bg = CssValues.Color(theme.Background, "#0a0a0b");
        switch (theme.BackgroundMode)
        {
            case "gradient":
            {
                List<string> stops = new() { CssValues.Color(theme.GradientFrom, bg) };
                if (CssValues.IsColor(theme.GradientVia))
                {
                    stops.Add(theme.GradientVia.Trim());
                }

                stops.Add(CssValues.Color(theme.GradientTo, bg));
                int angle = CssValues.Clamp(theme.GradientAngle, 0, 360);
                return $"background-color:{bg};background-image:linear-gradient({angle}deg, {string.Join(", ", stops)});";
            }
            case "image" when CssValues.IsUrl(theme.BackgroundImageUrl):
            {
                string dim = (Math.Max(CssValues.Clamp(theme.BackgroundDim, 0, 95), 35) / 100.0).ToString(CultureInfo.InvariantCulture);
                return $"background-color:{bg};" +
                       $"background-image:linear-gradient(rgba(0,0,0,{dim}), rgba(0,0,0,{dim})), url(\"{api.Resolve(theme.BackgroundImageUrl)}\");" +
                       "background-size:cover;background-position:center;";
            }
            case "pattern":
                return $"background-color:{bg};{Pattern(theme.Pattern)}";
            default:
                return $"background-color:{bg};";
        }
    }

    public static string Pattern(string? pattern)
    {
        return pattern switch
        {
            "halftone" => "background-image:radial-gradient(var(--pattern-color) 30%, transparent 32%);background-size:9px 9px;",
            "dots" => "background-image:radial-gradient(var(--pattern-color) 1.5px, transparent 2px);background-size:22px 22px;",
            "scanlines" => "background-image:repeating-linear-gradient(0deg, var(--pattern-color) 0 1px, transparent 1px 4px);",
            "noise" => $"background-image:{NoiseSvg};",
            _ => "background-image:linear-gradient(var(--pattern-color) 1px, transparent 1px), linear-gradient(90deg, var(--pattern-color) 1px, transparent 1px);background-size:40px 40px;"
        };
    }

    public static string Classes(ProfileThemeDto theme)
    {
        List<string> classes = new()
        {
            "cards-" + CssValues.OneOf(theme.CardStyle, "solid", "solid", "glass", "outline", "brutal", "hud", "bare"),
            "avatar-" + CssValues.OneOf(theme.AvatarShape, "circle", "circle", "square", "hex", "star"),
            "name-" + CssValues.OneOf(theme.NameAlign, "left", "left", "center", "right"),
            "layout-" + CssValues.OneOf(theme.Layout, "classic", Layouts),
            "namefx-" + CssValues.OneOf(theme.NameStyle, "plain", NameStyles),
            "motion-" + CssValues.OneOf(theme.ModuleMotion, "rise", Motions),
            "fit-" + CssValues.OneOf(theme.BannerFit, "cover", "cover", "full")
        };
        if (theme.Grain) classes.Add("fx-grain");
        if (theme.Scanlines) classes.Add("fx-scanlines");
        if (theme.Halftone) classes.Add("fx-halftone");
        if (theme.Chromatic) classes.Add("fx-chroma");
        if (theme.HudLabels) classes.Add("fx-hud");
        return string.Join(" ", classes);
    }

    public static readonly string[] Layouts = { "classic", "sidebar", "centered", "magazine", "overlay" };
    public static readonly string[] NameStyles = { "plain", "glitch", "chrome", "outline", "neon", "typewriter" };
    public static readonly string[] Motions = { "none", "rise", "fade", "pop", "tilt" };
    public static readonly string[] Decorations =
    {
        "none", "ring", "double", "glow", "aura", "holo", "scan", "hud", "sigil", "orbit", "crown", "spark", "pixel"
    };
    public static readonly string[] AnimatedBackgrounds = { "none", "flow", "starfield", "dither", "grid", "rain" };

    // where the avatar sits on the banner: center X/Y in % of the banner, width in % of the banner's width
    public static string AvatarPosition(ProfileThemeDto theme)
    {
        return $"--ax:{CssValues.Clamp(theme.AvatarX, 0, 100)}%;" +
               $"--ay:{CssValues.Clamp(theme.AvatarY, 0, 160)}%;" +
               $"--aw:{CssValues.Clamp(theme.AvatarSize, 4, 45)}%;" +
               $"--awn:{CssValues.Clamp(theme.AvatarSize, 4, 45)};";
    }

    public static List<string> FontUrls(params string?[] fonts)
    {
        return fonts
            .Select(f => CssValues.Font(f, ""))
            .Where(f => f.Length > 0 && !shellFonts.Contains(f))
            .Distinct()
            .Select(f => $"https://fonts.googleapis.com/css2?family={f.Replace(' ', '+')}&display=swap")
            .ToList();
    }

    public static List<string> FontUrls(ProfileThemeDto theme)
    {
        return FontUrls(theme.DisplayFont, theme.BodyFont, theme.MonoFont);
    }

    public static string ModuleVariables(ModuleStyleDto style)
    {
        StringBuilder css = new();
        if (CssValues.IsColor(style.Surface))
        {
            css.Append($"--surface:{style.Surface!.Trim()};");
            css.Append($"--surface-2:color-mix(in srgb, {style.Surface.Trim()} 84%, var(--text));");
        }

        if (CssValues.IsColor(style.Text)) css.Append($"--text:{style.Text!.Trim()};");
        if (CssValues.IsColor(style.Accent))
        {
            css.Append($"--accent:{style.Accent!.Trim()};");
            css.Append($"--on-accent:{ContrastText(style.Accent.Trim())};");
        }

        if (CssValues.IsColor(style.Border)) css.Append($"--line:{style.Border!.Trim()};");
        if (style.Radius is not null) css.Append($"--radius:{CssValues.Clamp(style.Radius.Value, 0, 40)}px;");
        if (style.BorderWidth is not null) css.Append($"--border-w:{CssValues.Clamp(style.BorderWidth.Value, 0, 8)}px;");
        if (style.GlowStrength is not null) css.Append($"--glow:{CssValues.Clamp(style.GlowStrength.Value, 0, 60)}px;");
        string display = CssValues.Font(style.DisplayFont, "");
        if (display.Length > 0) css.Append($"--font-display:'{display}', sans-serif;");
        string body = CssValues.Font(style.BodyFont, "");
        if (body.Length > 0) css.Append($"--font-body:'{body}', sans-serif;");
        if (style.Tilt is not null) css.Append($"--tilt:{CssValues.Clamp(style.Tilt.Value, -10, 10)}deg;");
        return css.ToString();
    }

    public static string ModuleClasses(ModuleStyleDto style)
    {
        List<string> classes = new();
        if (style.CardStyle is not null)
        {
            classes.Add("module-cards-" + CssValues.OneOf(style.CardStyle, "solid", "solid", "glass", "outline", "brutal", "hud", "bare"));
        }

        classes.Add("pad-" + CssValues.OneOf(style.Padding, "normal", "compact", "normal", "roomy"));
        classes.Add("height-" + CssValues.OneOf(style.Height, "auto", "auto", "short", "medium", "tall"));
        return string.Join(" ", classes);
    }

    // black or white text, whichever reads better on the given hex color
    public static string ContrastText(string color)
    {
        string hex = color.TrimStart('#');
        if (hex.Length is 3 or 4)
        {
            hex = string.Concat(hex[..3].Select(c => $"{c}{c}"));
        }

        if (hex.Length < 6 || !int.TryParse(hex[..6], NumberStyles.HexNumber, null, out int rgb))
        {
            return "#ffffff";
        }

        double r = ((rgb >> 16) & 255) / 255.0;
        double g = ((rgb >> 8) & 255) / 255.0;
        double b = (rgb & 255) / 255.0;
        double luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
        return luminance > 0.55 ? "#0a0a0b" : "#ffffff";
    }
}
