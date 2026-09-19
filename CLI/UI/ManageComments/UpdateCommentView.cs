using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageComments;

public class UpdateCommentView
{
    private readonly ICommentRepository commentRepository;

    public UpdateCommentView(ICommentRepository commentRepository)
    {
        this.commentRepository = commentRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Update comment");
        Console.Write("Comment ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out int id))
        {
            Console.WriteLine("Comment ID must be a number.");
            return;
        }

        Comment? comment = commentRepository.GetMany().SingleOrDefault(c => c.Id == id);
        if (comment is null)
        {
            Console.WriteLine($"Comment with ID {id} does not exist.");
            return;
        }

        Console.WriteLine($"Current text: {comment.Body}");
        Console.Write("New text: ");
        string? body = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Comment is required.");
            return;
        }

        comment.Body = body;
        await commentRepository.UpdateAsync(comment);
        Console.WriteLine("Comment updated.");
    }
}