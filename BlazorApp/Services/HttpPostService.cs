using ApiContracts;

namespace BlazorApp.Services;

public class HttpPostService : IPostService
{
    private readonly HttpClient client;

    public HttpPostService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<PostDto> AddPostAsync(CreatePostDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("posts", request);
        return await HttpResponseReader.ReadAsync<PostDto>(httpResponse);
    }

    public async Task UpdatePostAsync(int id, UpdatePostDto request)
    {
        HttpResponseMessage httpResponse = await client.PutAsJsonAsync($"posts/{id}", request);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task<PostDto> GetPostAsync(int id, bool includeComments = false)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"posts/{id}?includeComments={includeComments}");
        return await HttpResponseReader.ReadAsync<PostDto>(httpResponse);
    }

    public async Task<List<PostDto>> GetPostsAsync(PostQuery? query = null)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"posts{query?.ToQueryString()}");
        return await HttpResponseReader.ReadAsync<List<PostDto>>(httpResponse);
    }

    public async Task DeletePostAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"posts/{id}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task LikeAsync(int postId, int userId)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"posts/{postId}/likes", new LikeDto { UserId = userId });
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task UnlikeAsync(int postId, int userId)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"posts/{postId}/likes/{userId}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task AddTagAsync(int postId, int tagId)
    {
        HttpResponseMessage httpResponse = await client.PostAsync($"posts/{postId}/tags/{tagId}", null);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task RemoveTagAsync(int postId, int tagId)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"posts/{postId}/tags/{tagId}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }
}
