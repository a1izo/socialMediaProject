using ApiContracts;

namespace BlazorApp.Services;

public class HttpUserService : IUserService
{
    private readonly HttpClient client;

    public HttpUserService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<UserDto> AddUserAsync(CreateUserDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("users", request);
        return await HttpResponseReader.ReadAsync<UserDto>(httpResponse);
    }

    public async Task UpdateUserAsync(int id, UpdateUserDto request)
    {
        HttpResponseMessage httpResponse = await client.PutAsJsonAsync($"users/{id}", request);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"users/{id}");
        return await HttpResponseReader.ReadAsync<UserDto>(httpResponse);
    }

    public async Task<List<UserDto>> GetUsersAsync(string? userNameContains = null)
    {
        string query = string.IsNullOrWhiteSpace(userNameContains)
            ? ""
            : $"?userNameContains={Uri.EscapeDataString(userNameContains)}";
        HttpResponseMessage httpResponse = await client.GetAsync($"users{query}");
        return await HttpResponseReader.ReadAsync<List<UserDto>>(httpResponse);
    }

    public async Task DeleteUserAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"users/{id}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task<List<UserDto>> GetFollowersAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"users/{id}/followers");
        return await HttpResponseReader.ReadAsync<List<UserDto>>(httpResponse);
    }

    public async Task<List<UserDto>> GetFollowingAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"users/{id}/following");
        return await HttpResponseReader.ReadAsync<List<UserDto>>(httpResponse);
    }

    public async Task FollowAsync(int id, int followingId)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"users/{id}/following",
            new FollowDto { FollowingId = followingId });
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task UnfollowAsync(int id, int followingId)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"users/{id}/following/{followingId}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }
}
