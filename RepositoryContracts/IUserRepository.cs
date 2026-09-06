using Entities;

namespace RepositoryContracts;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<User> GetSingleAsync(int id);
    IQueryable<User> GetMany();

    //following logic
    Task FollowAsync(int followerId, int followingId);
    Task UnfollowAsync(int followerId, int followingId);
    IQueryable<User> GetFollowersAsync(int userId);
    IQueryable<User> GetFollowingAsync(int userId);
}