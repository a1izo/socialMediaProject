using ApiContracts;

namespace BlazorApp.Services;

public interface IUserService
{
    public Task<UserDto> AddUserAsync(CreateUserDto request);
    public Task UpdateUserAsync(int id, UpdateUserDto request);
    public Task<UserDto> GetUserAsync(int id);
    public Task<List<UserDto>> GetUsersAsync(string? userNameContains = null);
    public Task DeleteUserAsync(int id);
    public Task<List<UserDto>> GetFollowersAsync(int id);
    public Task<List<UserDto>> GetFollowingAsync(int id);
    public Task FollowAsync(int id, int followingId);
    public Task UnfollowAsync(int id, int followingId);
}
