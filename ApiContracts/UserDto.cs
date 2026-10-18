namespace ApiContracts;

public class UserDto
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsModerator { get; set; }
    public DateTime CreatedDate { get; set; }
}
