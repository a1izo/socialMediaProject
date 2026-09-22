using CLI.UI;
using FileRepositories;
using RepositoryContracts;

Console.WriteLine("Starting CLI app...");

ITagRepository tagRepository = new TagFileRepository();
IUserRepository userRepository = new UserFileRepository();
IPostRepository postRepository = new PostFileRepository(tagRepository);
ICommentRepository commentRepository = new CommentFileRepository();

CliApp app = new(userRepository, postRepository, commentRepository);
await app.StartAsync();