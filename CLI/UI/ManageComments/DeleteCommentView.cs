using RepositoryContracts;

namespace CLI.UI.ManageComments;

public class DeleteCommentView
{
    private readonly ICommentRepository commentRepository;

    public DeleteCommentView(ICommentRepository commentRepository)
    {
        this.commentRepository = commentRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Delete comment");
        Console.Write("Comment ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out int id))
        {
            Console.WriteLine("Comment ID must be a number.");
            return;
        }

        if (!commentRepository.GetMany().Any(c => c.Id == id))
        {
            Console.WriteLine($"Comment with ID {id} does not exist.");
            return;
        }

        await commentRepository.DeleteAsync(id);
        Console.WriteLine("Comment deleted.");
    }
}