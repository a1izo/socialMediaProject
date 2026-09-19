using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostsView
{
    private readonly CreatePostView createPostView;
    private readonly ListPostsView listPostsView;
    private readonly SinglePostView singlePostView;
    private readonly UpdatePostView updatePostView;
    private readonly DeletePostView deletePostView;

    public ManagePostsView(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        createPostView = new CreatePostView(postRepository, userRepository);
        listPostsView = new ListPostsView(postRepository);
        singlePostView = new SinglePostView(postRepository, userRepository, commentRepository);
        updatePostView = new UpdatePostView(postRepository);
        deletePostView = new DeletePostView(postRepository);
    }

    public async Task ShowAsync()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("Manage posts");
            Console.WriteLine("1. Create post");
            Console.WriteLine("2. List posts");
            Console.WriteLine("3. View post");
            Console.WriteLine("4. Update post");
            Console.WriteLine("5. Delete post");
            Console.WriteLine("0. Back");
            Console.Write("Choice: ");

            string? input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await createPostView.ShowAsync();
                    break;
                case "2":
                    await listPostsView.ShowAsync();
                    break;
                case "3":
                    await singlePostView.ShowAsync();
                    break;
                case "4":
                    await updatePostView.ShowAsync();
                    break;
                case "5":
                    await deletePostView.ShowAsync();
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