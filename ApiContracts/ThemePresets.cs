using System.Text.Json;

namespace ApiContracts;

// Complete looks: colors, fonts, background, layout, effects and header style in one click.
public static class ThemePresets
{
    public const string Default = "void";

    private static readonly Dictionary<string, ProfileThemeDto> presets = new()
    {
        ["void"] = new ProfileThemeDto
        {
            Preset = "void",
            Background = "#0a0a0b", Surface = "#121215", Text = "#ecebe6", Muted = "#85858f",
            Accent = "#ff2a4f", Accent2 = "#2f45ff", Border = "#2a2a31",
            BackgroundMode = "pattern", Pattern = "grid", PatternColor = "#16161b",
            DisplayFont = "Unbounded", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "hud", Radius = 2, BorderWidth = 1, GlowColor = "#ff2a4f",
            Grain = true, Scanlines = true, HudLabels = true, Sparkles = true, SparkleDensity = 1,
            Layout = "classic", ModuleMotion = "rise", NameStyle = "glitch", AvatarDecoration = "hud",
            AvatarShape = "square"
        },
        ["paper"] = new ProfileThemeDto
        {
            Preset = "paper",
            Background = "#e8e6e1", Surface = "#f2f0eb", Text = "#111111", Muted = "#5f5d58",
            Accent = "#111111", Accent2 = "#e5122f", Border = "#1a1a1a",
            DisplayFont = "Syne", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "outline", Radius = 0, BorderWidth = 1, GlowColor = "#111111",
            Grain = true, EffectIntensity = 60, HudLabels = true,
            Layout = "centered", ModuleMotion = "fade", NameStyle = "plain", AvatarDecoration = "glow", AvatarShape = "circle"
        },
        ["sigil"] = new ProfileThemeDto
        {
            Preset = "sigil",
            Background = "#1f2dff", Surface = "#0e169966", Text = "#f4f4ff", Muted = "#b4b9ff",
            Accent = "#ffffff", Accent2 = "#c7c9d6", Border = "#9aa0ff",
            BackgroundMode = "gradient", GradientFrom = "#2433ff", GradientVia = "#1a24d6", GradientTo = "#070b5c", GradientAngle = 160,
            DisplayFont = "Unbounded", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "glass", Radius = 14, BorderWidth = 1, GlowColor = "#ffffff", GlowStrength = 10,
            Grain = true, Sparkles = true, SparkleDensity = 2, HudLabels = true,
            Layout = "classic", ModuleMotion = "pop", NameStyle = "chrome", AvatarDecoration = "sigil", AvatarShape = "star"
        },
        ["halftone"] = new ProfileThemeDto
        {
            Preset = "halftone",
            Background = "#efe9dc", Surface = "#f7f2e6", Text = "#1c2366", Muted = "#4b5290",
            Accent = "#1c2366", Accent2 = "#e8b923", Border = "#1c2366",
            BackgroundMode = "pattern", Pattern = "halftone", PatternColor = "#d9d1bf",
            DisplayFont = "Archivo Black", BodyFont = "Space Grotesk", MonoFont = "Space Mono",
            CardStyle = "brutal", Radius = 0, BorderWidth = 2, GlowColor = "#1c2366",
            Grain = true, Halftone = true, EffectIntensity = 60,
            Layout = "magazine", ModuleMotion = "rise", NameStyle = "outline", AvatarShape = "circle"
        },
        ["signal"] = new ProfileThemeDto
        {
            Preset = "signal",
            Background = "#e3152b", Surface = "#f1efe9", Text = "#0b0b0b", Muted = "#4a4a4a",
            Accent = "#e3152b", Accent2 = "#0b0b0b", Border = "#0b0b0b",
            DisplayFont = "Anton", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            HeadingCase = "upper", LetterSpacing = 1,
            CardStyle = "brutal", Radius = 0, BorderWidth = 3, GlowColor = "#0b0b0b",
            Grain = true, HudLabels = true,
            Layout = "overlay", ModuleMotion = "tilt", NameStyle = "plain", AvatarShape = "hex", BannerHeight = 340
        },
        ["bloom"] = new ProfileThemeDto
        {
            Preset = "bloom",
            Background = "#1f3d1c", Surface = "#1d2a1799", Text = "#f6f0dc", Muted = "#cfc6a6",
            Accent = "#f2c94c", Accent2 = "#e88aa5", Border = "#f6f0dc40",
            BackgroundMode = "gradient", GradientFrom = "#1f3d1c", GradientVia = "#5d6e26", GradientTo = "#d9b44a", GradientAngle = 200,
            DisplayFont = "Instrument Serif", BodyFont = "Inter Tight", MonoFont = "JetBrains Mono",
            CardStyle = "glass", Radius = 22, BorderWidth = 1, GlowColor = "#f2c94c", GlowStrength = 18,
            Grain = true, Sparkles = true, SparkleDensity = 2, HudLabels = false, AnimatedBackground = "flow",
            Layout = "centered", ModuleMotion = "fade", NameStyle = "plain", AvatarDecoration = "crown", AvatarShape = "circle"
        },
        ["chrome"] = new ProfileThemeDto
        {
            Preset = "chrome",
            Background = "#c9ccd3", Surface = "#eef0f4cc", Text = "#111318", Muted = "#5a5f6b",
            Accent = "#3b4cff", Accent2 = "#ff3bd4", Border = "#9aa0ad",
            BackgroundMode = "gradient", GradientFrom = "#f4f5f8", GradientVia = "#aeb3bd", GradientTo = "#e9ebef", GradientAngle = 135,
            DisplayFont = "Michroma", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "glass", Radius = 18, BorderWidth = 1, GlowColor = "#ffffff", GlowStrength = 12,
            Grain = true, Sparkles = true, SparkleDensity = 2, Chromatic = true,
            Layout = "classic", ModuleMotion = "pop", NameStyle = "chrome", AvatarDecoration = "holo", AvatarShape = "circle"
        },
        ["arcade"] = new ProfileThemeDto
        {
            Preset = "arcade",
            Background = "#050805", Surface = "#0b140bdd", Text = "#b8ffb0", Muted = "#5fa85a",
            Accent = "#39ff14", Accent2 = "#ff2bd6", Border = "#1f3a1d",
            DisplayFont = "Press Start 2P", BodyFont = "Space Mono", MonoFont = "VT323", BaseFontSize = 15,
            CardStyle = "outline", Radius = 0, BorderWidth = 2, GlowColor = "#39ff14", GlowStrength = 14,
            Scanlines = true, EffectIntensity = 75, HudLabels = true, AnimatedBackground = "rain",
            Layout = "sidebar", ModuleMotion = "pop", NameStyle = "neon", AvatarDecoration = "pixel", AvatarShape = "square"
        },
        ["zine"] = new ProfileThemeDto
        {
            Preset = "zine",
            Background = "#f3efe6", Surface = "#fffdf7", Text = "#161616", Muted = "#5b5b5b",
            Accent = "#ff4f1f", Accent2 = "#1f6bff", Border = "#161616",
            BackgroundMode = "pattern", Pattern = "dots", PatternColor = "#d8d2c3",
            DisplayFont = "Bebas Neue", BodyFont = "Bricolage Grotesque", MonoFont = "Space Mono",
            HeadingCase = "upper", LetterSpacing = 2,
            CardStyle = "brutal", Radius = 0, BorderWidth = 2, GlowColor = "#161616",
            Grain = true, Halftone = true, EffectIntensity = 45,
            Layout = "magazine", ModuleMotion = "tilt", NameStyle = "typewriter", AvatarShape = "square"
        },
        ["dither"] = new ProfileThemeDto
        {
            Preset = "dither",
            Background = "#000000", Surface = "#000000", Text = "#ffffff", Muted = "#9a9a9a",
            Accent = "#ffffff", Accent2 = "#7d7d7d", Border = "#ffffff",
            DisplayFont = "Silkscreen", BodyFont = "Space Mono", MonoFont = "Space Mono",
            CardStyle = "outline", Radius = 0, BorderWidth = 1, GlowColor = "#ffffff",
            Halftone = true, EffectIntensity = 70, HudLabels = true, AnimatedBackground = "dither",
            Layout = "classic", ModuleMotion = "pop", NameStyle = "glitch", AvatarDecoration = "pixel", AvatarShape = "square"
        },
        ["aurora"] = new ProfileThemeDto
        {
            Preset = "aurora",
            Background = "#07071a", Surface = "#12123088", Text = "#f2f0ff", Muted = "#a9a6cc",
            Accent = "#7cf7d4", Accent2 = "#b48cff", Border = "#ffffff26",
            DisplayFont = "Unbounded", BodyFont = "Inter Tight", MonoFont = "JetBrains Mono",
            CardStyle = "glass", Radius = 20, BorderWidth = 1, GlowColor = "#7cf7d4", GlowStrength = 20,
            Grain = true, Sparkles = true, SparkleDensity = 1, AnimatedBackground = "flow",
            Layout = "centered", ModuleMotion = "fade", NameStyle = "neon", AvatarDecoration = "aura", AvatarShape = "circle"
        },
        ["blood moon"] = new ProfileThemeDto
        {
            Preset = "blood moon",
            Background = "#0d0405", Surface = "#1a0809", Text = "#f5e6e0", Muted = "#a17b74",
            Accent = "#d10f2f", Accent2 = "#ffb38a", Border = "#3a1115",
            DisplayFont = "DM Serif Display", BodyFont = "Inter Tight", MonoFont = "JetBrains Mono",
            CardStyle = "solid", Radius = 6, BorderWidth = 1, GlowColor = "#d10f2f", GlowStrength = 8,
            Grain = true, Sparkles = true, SparkleDensity = 1, AnimatedBackground = "starfield",
            Layout = "overlay", ModuleMotion = "rise", NameStyle = "plain", AvatarDecoration = "sigil", AvatarShape = "circle",
            BannerHeight = 360
        },
        ["synth"] = new ProfileThemeDto
        {
            Preset = "synth",
            Background = "#12002a", Surface = "#1d0640cc", Text = "#fdf2ff", Muted = "#b79bd6",
            Accent = "#ff2bd6", Accent2 = "#00e5ff", Border = "#ff2bd655",
            DisplayFont = "Orbitron", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "hud", Radius = 4, BorderWidth = 1, GlowColor = "#ff2bd6", GlowStrength = 16,
            Scanlines = true, EffectIntensity = 40, HudLabels = true, AnimatedBackground = "grid",
            Layout = "classic", ModuleMotion = "rise", NameStyle = "chrome", AvatarDecoration = "scan", AvatarShape = "hex"
        }
    };

    public static IReadOnlyList<string> Names { get; } = presets.Keys.ToList();

    public static ProfileThemeDto Create(string? name)
    {
        ProfileThemeDto source = presets.GetValueOrDefault((name ?? Default).ToLower(), presets[Default]);
        ProfileThemeDto theme = Clone(source);
        ApplyAvatarDefaults(theme);
        return theme;
    }

    // Each layout has its own good spot for the avatar, chosen so it never covers the name:
    // overlapping the banner's bottom edge for most layouts, floating in the top right of the
    // image for "overlay" (where the name sits over the bottom of the banner).
    public static void ApplyAvatarDefaults(ProfileThemeDto theme)
    {
        (theme.AvatarX, theme.AvatarY, theme.AvatarSize) = theme.Layout switch
        {
            "centered" => (50, 100, 16),
            "sidebar" => (13, 100, 14),
            "magazine" => (88, 100, 13),
            "overlay" => (86, 30, 14),
            _ => (9, 100, 12)
        };

        if (theme.Layout == "centered")
        {
            theme.NameAlign = "center";
        }
        else if (theme.Layout == "magazine")
        {
            theme.NameAlign = "left";
        }
    }

    // Applies a look but keeps the things that belong to the person, not the look: their song,
    // and, when they show the whole banner image (to line the avatar up with a frame drawn into it),
    // the banner settings and avatar position. Otherwise the avatar moves to the look's layout spot.
    public static ProfileThemeDto Apply(ProfileThemeDto current, string name)
    {
        ProfileThemeDto next = Create(name);
        next.SongUrl = current.SongUrl;
        if (current.BannerFit == "full")
        {
            next.BannerFit = "full";
            next.BannerHeight = current.BannerHeight;
            next.AvatarX = current.AvatarX;
            next.AvatarY = current.AvatarY;
            next.AvatarSize = current.AvatarSize;
        }

        return next;
    }

    public static ProfileThemeDto Clone(ProfileThemeDto theme)
    {
        return JsonSerializer.Deserialize<ProfileThemeDto>(JsonSerializer.Serialize(theme))!;
    }
}
