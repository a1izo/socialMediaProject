using ApiContracts;
using BlazorApp.Theming;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Components.Profile;

// Every profile module receives the same three things: the profile it lives on,
// its own module record, and its parsed settings.
public abstract class ProfileModuleBase : ComponentBase
{
    [Parameter] public ProfileDto Profile { get; set; } = default!;
    [Parameter] public ProfileModuleDto Module { get; set; } = default!;
    [Parameter] public ModuleSettings Settings { get; set; } = new();
    [Parameter] public bool Editing { get; set; }

    private string? loadedKey;

    // Modules that fetch data call this from OnParametersSetAsync. It returns true only
    // when the settings (or the profile) actually changed since the last load.
    protected bool ShouldReload()
    {
        string key = $"{Profile.UserId}|{Module.SettingsJson}";
        if (key == loadedKey)
        {
            return false;
        }

        loadedKey = key;
        return true;
    }
}
