using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class ListUsersView
{
    private readonly IUserRepository userRepository;

    public ListUsersView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("List users");
        Console.Write("Filter by user name (leave empty for all): ");
        string? filter = Console.ReadLine();

        IQueryable<User> query = userRepository.GetMany();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(u => u.UserName.ToLower().Contains(filter.ToLower()));
        }

        List<User> users = query.ToList();
        if (users.Count == 0)
        {
            Console.WriteLine("No users found.");
            return Task.CompletedTask;
        }

        foreach (User user in users)
        {
            Console.WriteLine($"[{user.Id}] {user.UserName}");
        }

        return Task.CompletedTask;
    }
}