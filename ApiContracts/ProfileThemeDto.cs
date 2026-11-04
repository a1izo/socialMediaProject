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

    // 0 to 100, how strong grain, scanlines and halftone look
    public int EffectIntensity { get; set; } = 50;
    // none | flow | starfield | dither | grid | rain
    public string AnimatedBackground { get; set; } = "none";

    // layout: classic | sidebar | centered | magazine | overlay
    public string Layout { get; set; } = "classic";
    // none | rise | fade | pop | tilt
    public string ModuleMotion { get; set; } = "rise";

    // header
    public int BannerHeight { get; set; } = 240;
    // cover: crop the banner to BannerHeight. full: show the whole image at its own shape
    public string BannerFit { get; set; } = "cover";
    // avatar center, in % of the banner's width (X) and height (Y), and its size in % of the banner's width.
    // Percentages keep a frame drawn into the banner lined up at every screen width.
    public int AvatarX { get; set; } = 9;
    public int AvatarY { get; set; } = 100;
    public int AvatarSize { get; set; } = 12;
    // plain | glitch | chrome | outline | neon | typewriter
    public string NameStyle { get; set; } = "plain";
    // none | ring | double | sigil | orbit | spark
    public string AvatarDecoration { get; set; } = "none";
    // youtube or spotify link, played only when the visitor presses play
    public string? SongUrl { get; set; }
    // circle | square | hex | star
    public string AvatarShape { get; set; } = "circle";
    public int AvatarBorder { get; set; } = 3;
    // left | center | right
    public string NameAlign { get; set; } = "left";
}
