using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class DeleteUserView
{
    private readonly IUserRepository userRepository;

    public DeleteUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Delete user");
        Console.Write("User ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out int id))
        {
            Console.WriteLine("User ID must be a number.");
            return;
        }

        if (!userRepository.GetMany().Any(u => u.Id == id))
        {
            Console.WriteLine($"User with ID {id} does not exist.");
            return;
        }

        await userRepository.DeleteAsync(id);
        Console.WriteLine("User deleted.");
    }
}