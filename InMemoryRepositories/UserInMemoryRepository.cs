using Entities;
using RepositoryContracts;

public class UserInMemoryRepository : IUserRepository
{
    private readonly List<User> users = new();
    private readonly List<Follow> follows = new();

    public Task<User> AddAsync(User user)
    {
        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        return Task.CompletedTask;
    }

    public Task<User> GetSingleAsync(int id)
    {
        User? user = users.SingleOrDefault(u => u.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        return Task.FromResult(user);
    }

    public IQueryable<User> GetMany()
    {
        return users.AsQueryable();
    }

    public Task FollowAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
        {
            throw new InvalidOperationException("A user cannot follow themselves");
        }

        bool alreadyFollowing = follows.Any(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (alreadyFollowing)
        {
            throw new InvalidOperationException(
                $"User '{followerId}' already follows user '{followingId}'");
        }

        Follow follow = new()
        {
            Id = follows.Any() ? follows.Max(f => f.Id) + 1 : 1,
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedDate = DateTime.Now
        };
        follows.Add(follow);

        return Task.CompletedTask;
    }

    public Task UnfollowAsync(int followerId, int followingId)
    {
        Follow? follow = follows.SingleOrDefault(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (follow is null)
        {
            throw new InvalidOperationException(
                $"User '{followerId}' does not follow user '{followingId}'");
        }

        follows.Remove(follow);
        return Task.CompletedTask;
    }

    public IQueryable<User> GetFollowersAsync(int userId)
    {
        List<int> followerIds = follows.Where(f => f.FollowingId == userId).Select(f => f.FollowerId).ToList();
        return users.Where(u => followerIds.Contains(u.Id)).AsQueryable();
    }

    public IQueryable<User> GetFollowingAsync(int userId)
    {
        List<int> followingIds = follows.Where(f => f.FollowerId == userId).Select(f => f.FollowingId).ToList();
        return users.Where(u => followingIds.Contains(u.Id)).AsQueryable();
    }
}