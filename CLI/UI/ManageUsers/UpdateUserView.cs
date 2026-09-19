using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class UpdateUserView
{
    private readonly IUserRepository userRepository;

    public UpdateUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Update user");
        Console.Write("User ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out int id))
        {
            Console.WriteLine("User ID must be a number.");
            return;
        }

        User? user = userRepository.GetMany().SingleOrDefault(u => u.Id == id);
        if (user is null)
        {
            Console.WriteLine($"User with ID {id} does not exist.");
            return;
        }

        Console.WriteLine("Leave a field empty to keep the current value.");

        Console.Write($"User name ({user.UserName}): ");
        string? userName = Console.ReadLine();
        Console.Write("Password: ");
        string? password = Console.ReadLine();
        Console.Write($"Email ({user.Email}): ");
        string? email = Console.ReadLine();
        Console.Write($"Phone number ({user.PhoneNumber}): ");
        string? phoneNumber = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(userName) &&
            userRepository.GetMany().Any(u => u.Id != id && u.UserName.ToLower() == userName.ToLower()))
        {
            Console.WriteLine("User name is already taken.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) &&
            userRepository.GetMany().Any(u => u.Id != id && u.Email.ToLower() == email.ToLower()))
        {
            Console.WriteLine("Email is already in use.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(userName)) user.UserName = userName;
        if (!string.IsNullOrWhiteSpace(password)) user.Password = password;
        if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
        if (!string.IsNullOrWhiteSpace(phoneNumber)) user.PhoneNumber = phoneNumber;

        await userRepository.UpdateAsync(user);
        Console.WriteLine("User updated.");
    }
}