namespace ApiContracts;

// Per-module overrides of the profile theme. Null means "inherit from the theme".
public class ModuleStyleDto
{
    public string? Surface { get; set; }
    public string? Text { get; set; }
    public string? Accent { get; set; }
    public string? Border { get; set; }
    public string? CardStyle { get; set; }
    public int? Radius { get; set; }
    public int? BorderWidth { get; set; }
    public int? GlowStrength { get; set; }
    public string? DisplayFont { get; set; }
    public string? BodyFont { get; set; }
    // compact | normal | roomy
    public string? Padding { get; set; }
    // auto | short | medium | tall
    public string? Height { get; set; }
    // -10 to 10 degrees
    public int? Tilt { get; set; }
}
