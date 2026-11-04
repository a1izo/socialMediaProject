namespace BlazorApp.Theming;

public record Palette(string Name, string Background, string Surface, string Text, string Muted, string Accent, string Accent2, string Border);

public record FontPairing(string Name, string Display, string Body, string Mono);

// Ready-made choices for the Style panel, so people pick from things that already look good
// instead of typing hex codes and font names.
public static class StyleOptions
{
    public static readonly Palette[] Palettes =
    {
        new("void", "#0a0a0b", "#121215", "#ecebe6", "#85858f", "#ff2a4f", "#2f45ff", "#2a2a31"),
        new("paper", "#e8e6e1", "#f2f0eb", "#111111", "#5f5d58", "#111111", "#e5122f", "#1a1a1a"),
        new("cobalt", "#1f2dff", "#0e169966", "#f4f4ff", "#b4b9ff", "#ffffff", "#c7c9d6", "#9aa0ff"),
        new("navy ink", "#efe9dc", "#f7f2e6", "#1c2366", "#4b5290", "#1c2366", "#e8b923", "#1c2366"),
        new("poster red", "#e3152b", "#f1efe9", "#0b0b0b", "#4a4a4a", "#e3152b", "#0b0b0b", "#0b0b0b"),
        new("moss", "#1f3d1c", "#1d2a1799", "#f6f0dc", "#cfc6a6", "#f2c94c", "#e88aa5", "#f6f0dc40"),
        new("chrome", "#c9ccd3", "#eef0f4cc", "#111318", "#5a5f6b", "#3b4cff", "#ff3bd4", "#9aa0ad"),
        new("terminal", "#050805", "#0b140bdd", "#b8ffb0", "#5fa85a", "#39ff14", "#ff2bd6", "#1f3a1d"),
        new("bubblegum", "#ffd6ec", "#fff0f8", "#3a0d2a", "#8c4a71", "#ff3d9a", "#7b5cff", "#ffadd6"),
        new("aurora", "#07071a", "#12123088", "#f2f0ff", "#a9a6cc", "#7cf7d4", "#b48cff", "#ffffff26"),
        new("blood moon", "#0d0405", "#1a0809", "#f5e6e0", "#a17b74", "#d10f2f", "#ffb38a", "#3a1115"),
        new("synth", "#12002a", "#1d0640cc", "#fdf2ff", "#b79bd6", "#ff2bd6", "#00e5ff", "#ff2bd655"),
        new("sand", "#e9dcc6", "#f4ebdc", "#3b2a1a", "#7d6650", "#c2541b", "#2f6b5a", "#cdb89a"),
        new("ice", "#eaf4fb", "#ffffffcc", "#0f2a3d", "#5b7a8f", "#1aa3ff", "#7fe0ff", "#bcd9ea"),
        new("mono", "#ffffff", "#f3f3f3", "#000000", "#666666", "#000000", "#999999", "#000000"),
        new("lime", "#101400", "#1b2200", "#f3ffd1", "#a7b880", "#d4ff00", "#ff6a00", "#3a4500")
    };

    public static readonly FontPairing[] FontPairings =
    {
        new("techno", "Unbounded", "Space Grotesk", "JetBrains Mono"),
        new("poster", "Anton", "Space Grotesk", "JetBrains Mono"),
        new("editorial", "Instrument Serif", "Inter Tight", "JetBrains Mono"),
        new("art school", "Syne", "Space Grotesk", "Space Mono"),
        new("zine", "Bebas Neue", "Bricolage Grotesque", "Space Mono"),
        new("gothic", "DM Serif Display", "Inter Tight", "IBM Plex Mono"),
        new("chrome", "Michroma", "Inter Tight", "JetBrains Mono"),
        new("sci-fi", "Orbitron", "Space Grotesk", "JetBrains Mono"),
        new("arcade", "Press Start 2P", "Space Mono", "VT323"),
        new("pixel", "Silkscreen", "Space Mono", "Space Mono"),
        new("heavy", "Archivo Black", "Space Grotesk", "Space Mono"),
        new("glitch", "Rubik Glitch", "Space Grotesk", "JetBrains Mono")
    };
}
