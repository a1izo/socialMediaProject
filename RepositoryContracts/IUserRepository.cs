using Entities;

namespace RepositoryContracts;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<User> GetSingleAsync(int id);
    IQueryable<User> GetMany();
    Task FollowAsync(int followerId, int followingId);
    Task UnfollowAsync(int followerId, int followingId);
    IQueryable<User> GetFollowers(int userId);
    IQueryable<User> GetFollowing(int userId);
}