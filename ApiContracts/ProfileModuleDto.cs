namespace ApiContracts;

public class ProfileModuleDto
{
    public int Id { get; set; }
    public required string Type { get; set; }
    public int Order { get; set; }
    public int Span { get; set; } = 6;
    public string? Title { get; set; }
    public bool ShowTitle { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public string SettingsJson { get; set; } = "{}";
    public ModuleStyleDto Style { get; set; } = new();
}
