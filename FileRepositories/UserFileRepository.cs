using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private readonly string usersFilePath = "users.json";
    private readonly string followsFilePath = "follows.json";

    public UserFileRepository()
    {
        JsonFileHelper.EnsureFileExists(usersFilePath);
        JsonFileHelper.EnsureFileExists(followsFilePath);
    }

    public async Task<User> AddAsync(User user)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(usersFilePath);
        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        await JsonFileHelper.SaveAsync(usersFilePath, users);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(usersFilePath);
        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);
        await JsonFileHelper.SaveAsync(usersFilePath, users);
    }

    public async Task DeleteAsync(int id)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(usersFilePath);
        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        await JsonFileHelper.SaveAsync(usersFilePath, users);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(usersFilePath);
        User? user = users.SingleOrDefault(u => u.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        List<User> users = JsonFileHelper.Load<User>(usersFilePath);
        return users.AsQueryable();
    }

    public async Task FollowAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
        {
            throw new InvalidOperationException("A user cannot follow themselves");
        }

        List<Follow> follows = await JsonFileHelper.LoadAsync<Follow>(followsFilePath);
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
        await JsonFileHelper.SaveAsync(followsFilePath, follows);
    }

    public async Task UnfollowAsync(int followerId, int followingId)
    {
        List<Follow> follows = await JsonFileHelper.LoadAsync<Follow>(followsFilePath);
        Follow? follow = follows.SingleOrDefault(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (follow is null)
        {
            throw new InvalidOperationException(
                $"User '{followerId}' does not follow user '{followingId}'");
        }

        follows.Remove(follow);
        await JsonFileHelper.SaveAsync(followsFilePath, follows);
    }

    public IQueryable<User> GetFollowers(int userId)
    {
        List<User> users = JsonFileHelper.Load<User>(usersFilePath);
        List<Follow> follows = JsonFileHelper.Load<Follow>(followsFilePath);

        List<int> followerIds = follows.Where(f => f.FollowingId == userId).Select(f => f.FollowerId).ToList();
        return users.Where(u => followerIds.Contains(u.Id)).AsQueryable();
    }

    public IQueryable<User> GetFollowing(int userId)
    {
        List<User> users = JsonFileHelper.Load<User>(usersFilePath);
        List<Follow> follows = JsonFileHelper.Load<Follow>(followsFilePath);

        List<int> followingIds = follows.Where(f => f.FollowerId == userId).Select(f => f.FollowingId).ToList();
        return users.Where(u => followingIds.Contains(u.Id)).AsQueryable();
    }
}
