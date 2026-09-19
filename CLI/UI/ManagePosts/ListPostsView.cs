using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ListPostsView
{
    private readonly IPostRepository postRepository;

    public ListPostsView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("List posts");
        Console.Write("Filter by title (leave empty for all): ");
        string? filter = Console.ReadLine();

        IQueryable<Post> query = postRepository.GetMany();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(p => p.Title.ToLower().Contains(filter.ToLower()));
        }

        List<Post> posts = query.ToList();
        if (posts.Count == 0)
        {
            Console.WriteLine("No posts found.");
            return Task.CompletedTask;
        }

        foreach (Post post in posts)
        {
            Console.WriteLine($"[{post.Id}] {post.Title}");
        }

        return Task.CompletedTask;
    }
}