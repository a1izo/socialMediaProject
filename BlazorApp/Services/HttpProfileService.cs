using ApiContracts;

namespace BlazorApp.Services;

public class HttpProfileService : IProfileService
{
    private readonly HttpClient client;

    public HttpProfileService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<ProfileDto> GetProfileAsync(string userName)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"profiles/{Uri.EscapeDataString(userName)}");
        return await HttpResponseReader.ReadAsync<ProfileDto>(httpResponse);
    }

    public async Task<List<ProfileCardDto>> GetCardsAsync(IEnumerable<int> userIds)
    {
        List<int> ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new List<ProfileCardDto>();
        }

        string query = string.Join("&", ids.Select(id => $"userIds={id}"));
        HttpResponseMessage httpResponse = await client.GetAsync($"profiles/cards?{query}");
        return await HttpResponseReader.ReadAsync<List<ProfileCardDto>>(httpResponse);
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileDto request)
    {
        HttpResponseMessage httpResponse = await client.PutAsJsonAsync($"profiles/{userId}", request);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }
}
