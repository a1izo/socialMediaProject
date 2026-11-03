using System.Text.Json;

namespace ApiContracts;

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
            CardStyle = "hud", Radius = 2, BorderWidth = 1, GlowColor = "#ff2a4f", GlowStrength = 0,
            Grain = true, Scanlines = true, HudLabels = true, Sparkles = true, SparkleDensity = 1,
            AvatarShape = "square", BannerHeight = 240
        },
        ["paper"] = new ProfileThemeDto
        {
            Preset = "paper",
            Background = "#e8e6e1", Surface = "#f2f0eb", Text = "#111111", Muted = "#5f5d58",
            Accent = "#111111", Accent2 = "#e5122f", Border = "#1a1a1a",
            BackgroundMode = "solid",
            DisplayFont = "Syne", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "outline", Radius = 0, BorderWidth = 1, GlowColor = "#111111",
            Grain = true, HudLabels = true, Sparkles = true, SparkleDensity = 1,
            AvatarShape = "circle", BannerHeight = 220
        },
        ["sigil"] = new ProfileThemeDto
        {
            Preset = "sigil",
            Background = "#1f2dff", Surface = "#0e169966", Text = "#f4f4ff", Muted = "#b4b9ff",
            Accent = "#ffffff", Accent2 = "#c7c9d6", Border = "#9aa0ff",
            BackgroundMode = "gradient", GradientFrom = "#2433ff", GradientVia = "#1a24d6", GradientTo = "#070b5c",
            GradientAngle = 160,
            DisplayFont = "Unbounded", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            CardStyle = "glass", Radius = 14, BorderWidth = 1, GlowColor = "#ffffff", GlowStrength = 10,
            Grain = true, Chromatic = true, Sparkles = true, SparkleDensity = 2, HudLabels = true,
            AvatarShape = "star", BannerHeight = 260
        },
        ["halftone"] = new ProfileThemeDto
        {
            Preset = "halftone",
            Background = "#efe9dc", Surface = "#f7f2e6", Text = "#1c2366", Muted = "#4b5290",
            Accent = "#1c2366", Accent2 = "#e8b923", Border = "#1c2366",
            BackgroundMode = "pattern", Pattern = "halftone", PatternColor = "#d9d1bf",
            DisplayFont = "Archivo Black", BodyFont = "Space Grotesk", MonoFont = "Space Mono",
            CardStyle = "brutal", Radius = 0, BorderWidth = 2, GlowColor = "#1c2366",
            Grain = true, Halftone = true, HudLabels = false,
            AvatarShape = "circle", BannerHeight = 240
        },
        ["signal"] = new ProfileThemeDto
        {
            Preset = "signal",
            Background = "#e3152b", Surface = "#f1efe9", Text = "#0b0b0b", Muted = "#4a4a4a",
            Accent = "#e3152b", Accent2 = "#0b0b0b", Border = "#0b0b0b",
            BackgroundMode = "solid",
            DisplayFont = "Anton", BodyFont = "Space Grotesk", MonoFont = "JetBrains Mono",
            HeadingCase = "upper", LetterSpacing = 1,
            CardStyle = "brutal", Radius = 0, BorderWidth = 3, GlowColor = "#0b0b0b",
            Grain = true, HudLabels = true,
            AvatarShape = "hex", BannerHeight = 300
        },
        ["bloom"] = new ProfileThemeDto
        {
            Preset = "bloom",
            Background = "#1f3d1c", Surface = "#1d2a1799", Text = "#f6f0dc", Muted = "#cfc6a6",
            Accent = "#f2c94c", Accent2 = "#e88aa5", Border = "#f6f0dc40",
            BackgroundMode = "gradient", GradientFrom = "#1f3d1c", GradientVia = "#5d6e26", GradientTo = "#d9b44a",
            GradientAngle = 200,
            DisplayFont = "Instrument Serif", BodyFont = "Inter Tight", MonoFont = "JetBrains Mono",
            CardStyle = "glass", Radius = 22, BorderWidth = 1, GlowColor = "#f2c94c", GlowStrength = 18,
            Grain = true, Sparkles = true, SparkleDensity = 2, HudLabels = false,
            AvatarShape = "circle", BannerHeight = 260, NameAlign = "center"
        }
    };

    public static IReadOnlyList<string> Names { get; } = presets.Keys.ToList();

    public static ProfileThemeDto Create(string? name)
    {
        ProfileThemeDto source = presets.GetValueOrDefault((name ?? Default).ToLower(), presets[Default]);
        return Clone(source);
    }

    public static ProfileThemeDto Clone(ProfileThemeDto theme)
    {
        return JsonSerializer.Deserialize<ProfileThemeDto>(JsonSerializer.Serialize(theme))!;
    }
}
