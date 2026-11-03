using ApiContracts;

namespace BlazorApp.Services;

public class HttpCommentService : ICommentService
{
    private readonly HttpClient client;

    public HttpCommentService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("comments", request);
        return await HttpResponseReader.ReadAsync<CommentDto>(httpResponse);
    }

    public async Task UpdateCommentAsync(int id, UpdateCommentDto request)
    {
        HttpResponseMessage httpResponse = await client.PutAsJsonAsync($"comments/{id}", request);
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task DeleteCommentAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"comments/{id}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task<List<CommentDto>> GetCommentsForPostAsync(int postId)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"posts/{postId}/comments");
        return await HttpResponseReader.ReadAsync<List<CommentDto>>(httpResponse);
    }

    public async Task LikeAsync(int commentId, int userId)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"comments/{commentId}/likes", new LikeDto { UserId = userId });
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }

    public async Task UnlikeAsync(int commentId, int userId)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"comments/{commentId}/likes/{userId}");
        await HttpResponseReader.EnsureSuccessAsync(httpResponse);
    }
}
