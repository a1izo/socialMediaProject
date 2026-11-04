using ApiContracts;

namespace BlazorApp.Services;

public class HttpSearchService : ISearchService
{
    private readonly HttpClient client;

    public HttpSearchService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<SearchResultDto> SearchAsync(string query, string type)
    {
        HttpResponseMessage httpResponse = await client.GetAsync(
            $"search?q={Uri.EscapeDataString(query)}&type={Uri.EscapeDataString(type)}");
        return await HttpResponseReader.ReadAsync<SearchResultDto>(httpResponse);
    }
}
