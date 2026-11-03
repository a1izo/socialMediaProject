using Microsoft.JSInterop;

namespace BlazorApp.Services;

// The site itself comes in two modes: "void" (dark) and "paper" (light).
// The choice is remembered in the browser's localStorage.
public class ShellThemeService
{
    private readonly IJSRuntime jsRuntime;

    public string Mode { get; private set; } = "void";

    public event Action? Changed;

    public ShellThemeService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            Mode = await jsRuntime.InvokeAsync<string>("cybernet.getShell");
            Changed?.Invoke();
        }
        catch (InvalidOperationException)
        {
            // js is not available yet
        }
    }

    public async Task ToggleAsync()
    {
        Mode = Mode == "void" ? "paper" : "void";
        await jsRuntime.InvokeVoidAsync("cybernet.setShell", Mode);
        Changed?.Invoke();
    }
}
