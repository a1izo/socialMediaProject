using ApiContracts;

namespace BlazorApp.Services;

public interface ISearchService
{
    // type: all | accounts | posts | tags
    public Task<SearchResultDto> SearchAsync(string query, string type);
}
