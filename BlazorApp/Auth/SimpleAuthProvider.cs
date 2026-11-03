using System.Security.Claims;
using System.Text.Json;
using ApiContracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorApp.Auth;

public class SimpleAuthProvider : AuthenticationStateProvider
{
    private const string StorageKey = "currentUser";

    private readonly HttpClient httpClient;
    private readonly IJSRuntime jsRuntime;

    // primary cache, so the browser's session storage is only read once per circuit
    private UserDto? cachedUser;

    public SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        this.httpClient = httpClient;
        this.jsRuntime = jsRuntime;
    }

    public async Task LoginAsync(string userName, string password)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest(userName, password));

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(content.Trim('"'));
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        await StoreUserAsync(userDto);
    }

    // used after the account settings change, so the claims show the new user name
    public async Task StoreUserAsync(UserDto userDto)
    {
        string serialisedData = JsonSerializer.Serialize(userDto);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, serialisedData);
        cachedUser = userDto;

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(CreatePrincipal(userDto)))
        );
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (cachedUser is not null)
        {
            return new AuthenticationState(CreatePrincipal(cachedUser));
        }

        string userAsJson = "";
        try
        {
            userAsJson = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", StorageKey);
        }
        catch (InvalidOperationException)
        {
            return new AuthenticationState(new());
        }

        if (string.IsNullOrEmpty(userAsJson))
        {
            return new AuthenticationState(new());
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(userAsJson)!;
        cachedUser = userDto;
        return new AuthenticationState(CreatePrincipal(userDto));
    }

    public async Task Logout()
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, "");
        cachedUser = null;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
    }

    private static ClaimsPrincipal CreatePrincipal(UserDto userDto)
    {
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new Claim("Id", userDto.Id.ToString()),
            new Claim("Email", userDto.Email),
            new Claim("IsModerator", userDto.IsModerator.ToString())
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, "apiauth");
        return new ClaimsPrincipal(identity);
    }
}
