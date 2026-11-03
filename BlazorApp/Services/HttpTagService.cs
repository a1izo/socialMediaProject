using ApiContracts;

namespace BlazorApp.Services;

public class HttpTagService : ITagService
{
    private readonly HttpClient client;

    public HttpTagService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<List<TagDto>> GetTagsAsync(string? nameContains = null)
    {
        string query = string.IsNullOrWhiteSpace(nameContains) ? "" : $"?nameContains={Uri.EscapeDataString(nameContains)}";
        HttpResponseMessage httpResponse = await client.GetAsync($"tags{query}");
        return await HttpResponseReader.ReadAsync<List<TagDto>>(httpResponse);
    }

    public async Task<TagDto> AddTagAsync(CreateTagDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("tags", request);
        return await HttpResponseReader.ReadAsync<TagDto>(httpResponse);
    }

    public async Task<TagDto> GetOrCreateTagAsync(string name)
    {
        string cleaned = name.Trim().ToLower();
        List<TagDto> existing = await GetTagsAsync(cleaned);
        TagDto? match = existing.FirstOrDefault(t => t.Name.ToLower() == cleaned);
        return match ?? await AddTagAsync(new CreateTagDto { Name = cleaned });
    }
}
