using CLI.UI;
using InMemoryRepositories;
using RepositoryContracts;

Console.WriteLine("Starting CLI app...");

ITagRepository tagRepository = new TagInMemoryRepository();
IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository(tagRepository);
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp app = new(userRepository, postRepository, commentRepository);
await app.StartAsync();