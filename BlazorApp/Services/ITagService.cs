using ApiContracts;

namespace BlazorApp.Services;

public interface ITagService
{
    public Task<List<TagDto>> GetTagsAsync(string? nameContains = null);
    public Task<TagDto> AddTagAsync(CreateTagDto request);
    public Task<TagDto> GetOrCreateTagAsync(string name);
}
