namespace ApiContracts;

public class UpdateUserDto
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? Password { get; set; }
}
