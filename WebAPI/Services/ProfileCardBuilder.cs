using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using WebAPI.Profiles;

namespace WebAPI.Services;

// Builds profile cards and stats. Used by the profiles and search controllers.
public class ProfileCardBuilder
{
    private readonly IProfileRepository profileRepo;
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;

    public ProfileCardBuilder(IProfileRepository profileRepo, IUserRepository userRepo, IPostRepository postRepo)
    {
        this.profileRepo = profileRepo;
        this.userRepo = userRepo;
        this.postRepo = postRepo;
    }

    public async Task<ProfileCardDto> BuildCardAsync(User user)
    {
        Profile profile = await ProfileDefaults.GetOrCreateAsync(profileRepo, user);
        return DtoMapper.ToCardDto(profile, user, await GetStatsAsync(user.Id));
    }

    public async Task<ProfileStatsDto> GetStatsAsync(int userId)
    {
        IQueryable<Post> posts = postRepo.GetMany().Where(p => p.UserId == userId);
        return new ProfileStatsDto
        {
            PostCount = await posts.CountAsync(),
            FollowerCount = await userRepo.GetFollowers(userId).CountAsync(),
            FollowingCount = await userRepo.GetFollowing(userId).CountAsync(),
            LikesReceived = await posts.SumAsync(p => p.Likes.Count)
        };
    }
}
