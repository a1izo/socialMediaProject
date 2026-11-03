namespace Entities;

public class ProfileModule
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; }
    public int Order { get; set; }
    public int Span { get; set; }
    public string? Title { get; set; }
    public bool ShowTitle { get; set; }
    public bool IsVisible { get; set; }
    public string SettingsJson { get; set; }
    public string StyleJson { get; set; }
}
