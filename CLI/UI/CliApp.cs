using CLI.UI.ManageComments;
using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly ManageUsersView manageUsersView;
    private readonly ManagePostsView managePostsView;
    private readonly ManageCommentsView manageCommentsView;

    public CliApp(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        manageUsersView = new ManageUsersView(userRepository);
        managePostsView = new ManagePostsView(userRepository, postRepository, commentRepository);
        manageCommentsView = new ManageCommentsView(userRepository, postRepository, commentRepository);
    }

    public async Task StartAsync()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("Main menu");
            Console.WriteLine("1. Manage users");
            Console.WriteLine("2. Manage posts");
            Console.WriteLine("3. Manage comments");
            Console.WriteLine("0. Exit");
            Console.Write("Choice: ");

            string? input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await manageUsersView.ShowAsync();
                    break;
                case "2":
                    await managePostsView.ShowAsync();
                    break;
                case "3":
                    await manageCommentsView.ShowAsync();
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