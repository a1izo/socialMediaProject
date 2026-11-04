namespace ApiContracts;

// Something placed freely on a profile: a glyph, an emoji or an uploaded image.
public class StickerDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    // glyph | emoji | image
    public string Kind { get; set; } = "glyph";
    // glyph name, the emoji itself, or an image url
    public string Value { get; set; } = "star";
    // horizontal position in % of the profile width, vertical position in px from the top
    public double X { get; set; } = 50;
    public int Y { get; set; } = 200;
    public int Size { get; set; } = 64;
    public int Rotation { get; set; }
    public string? Color { get; set; }
}
