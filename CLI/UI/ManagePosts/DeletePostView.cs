using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class DeletePostView
{
    private readonly IPostRepository postRepository;

    public DeletePostView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Delete post");
        Console.Write("Post ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out int id))
        {
            Console.WriteLine("Post ID must be a number.");
            return;
        }

        if (!postRepository.GetMany().Any(p => p.Id == id))
        {
            Console.WriteLine($"Post with ID {id} does not exist.");
            return;
        }

        await postRepository.DeleteAsync(id);
        Console.WriteLine("Post deleted.");
    }
}