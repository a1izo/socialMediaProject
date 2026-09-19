using RepositoryContracts;

namespace CLI.UI.ManageComments;

public class ManageCommentsView
{
    private readonly CreateCommentView createCommentView;
    private readonly ListCommentsView listCommentsView;
    private readonly UpdateCommentView updateCommentView;
    private readonly DeleteCommentView deleteCommentView;

    public ManageCommentsView(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        createCommentView = new CreateCommentView(postRepository, userRepository, commentRepository);
        listCommentsView = new ListCommentsView(userRepository, commentRepository);
        updateCommentView = new UpdateCommentView(commentRepository);
        deleteCommentView = new DeleteCommentView(commentRepository);
    }

    public async Task ShowAsync()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("Manage comments");
            Console.WriteLine("1. Add comment");
            Console.WriteLine("2. List comments");
            Console.WriteLine("3. Update comment");
            Console.WriteLine("4. Delete comment");
            Console.WriteLine("0. Back");
            Console.Write("Choice: ");

            string? input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await createCommentView.ShowAsync();
                    break;
                case "2":
                    await listCommentsView.ShowAsync();
                    break;
                case "3":
                    await updateCommentView.ShowAsync();
                    break;
                case "4":
                    await deleteCommentView.ShowAsync();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }
}