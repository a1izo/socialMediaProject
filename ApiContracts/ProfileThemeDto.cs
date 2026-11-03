namespace ApiContracts;

public class ProfileThemeDto
{
    public string Preset { get; set; } = "void";

    // colors
    public string Background { get; set; } = "#0a0a0b";
    public string Surface { get; set; } = "#121215";
    public string Text { get; set; } = "#ecebe6";
    public string Muted { get; set; } = "#85858f";
    public string Accent { get; set; } = "#ff2a4f";
    public string Accent2 { get; set; } = "#2f45ff";
    public string Border { get; set; } = "#2a2a31";

    // background: solid | gradient | image | pattern
    public string BackgroundMode { get; set; } = "solid";
    public string GradientFrom { get; set; } = "#0a0a0b";
    public string GradientVia { get; set; } = "";
    public string GradientTo { get; set; } = "#1a1a22";
    public int GradientAngle { get; set; } = 180;
    public string? BackgroundImageUrl { get; set; }
    public string BackgroundImageFit { get; set; } = "cover";
    public int BackgroundDim { get; set; } = 30;
    // halftone | grid | scanlines | noise | dots
    public string Pattern { get; set; } = "grid";
    public string PatternColor { get; set; } = "#1c1c22";

    // type
    public string DisplayFont { get; set; } = "Unbounded";
    public string BodyFont { get; set; } = "Space Grotesk";
    public string MonoFont { get; set; } = "JetBrains Mono";
    public int BaseFontSize { get; set; } = 16;
    // none | upper
    public string HeadingCase { get; set; } = "none";
    public int LetterSpacing { get; set; } = 0;

    // cards: solid | glass | outline | brutal | hud | bare
    public string CardStyle { get; set; } = "hud";
    public int Radius { get; set; } = 2;
    public int BorderWidth { get; set; } = 1;
    public string GlowColor { get; set; } = "#ff2a4f";
    public int GlowStrength { get; set; } = 0;
    public int GridGap { get; set; } = 16;

    // effects
    public bool Grain { get; set; } = true;
    public bool Scanlines { get; set; }
    public bool Halftone { get; set; }
    public bool Chromatic { get; set; }
    public bool Sparkles { get; set; }
    public bool SparkleTrail { get; set; }
    public int SparkleDensity { get; set; } = 2;
    public bool HudLabels { get; set; } = true;

    // header
    public int BannerHeight { get; set; } = 240;
    // circle | square | hex | star
    public string AvatarShape { get; set; } = "circle";
    public int AvatarBorder { get; set; } = 3;
    // left | center | right
    public string NameAlign { get; set; } = "left";
}
