using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Create user");

        Console.Write("User name: ");
        string? userName = Console.ReadLine();
        Console.Write("Password: ");
        string? password = Console.ReadLine();
        Console.Write("Email: ");
        string? email = Console.ReadLine();
        Console.Write("Phone number (optional): ");
        string? phoneNumber = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("User name, password and email are required.");
            return;
        }

        bool userNameTaken = userRepository.GetMany()
            .Any(u => u.UserName.ToLower() == userName.ToLower());
        if (userNameTaken)
        {
            Console.WriteLine("User name is already taken.");
            return;
        }

        bool emailTaken = userRepository.GetMany()
            .Any(u => u.Email.ToLower() == email.ToLower());
        if (emailTaken)
        {
            Console.WriteLine("Email is already in use.");
            return;
        }

        User user = new()
        {
            UserName = userName,
            Password = password,
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
            IsModerator = false,
            CreatedDate = DateTime.Now
        };

        User created = await userRepository.AddAsync(user);
        Console.WriteLine($"User created with ID {created.Id}.");
    }
}