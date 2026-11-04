using ApiContracts;
using Entities;
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
        List<int> postIds = postRepo.GetMany().Where(p => p.UserId == userId).Select(p => p.Id).ToList();
        int likesReceived = 0;
        foreach (int postId in postIds)
        {
            likesReceived += await postRepo.GetLikeCountAsync(postId);
        }

        return new ProfileStatsDto
        {
            PostCount = postIds.Count,
            FollowerCount = userRepo.GetFollowers(userId).Count(),
            FollowingCount = userRepo.GetFollowing(userId).Count(),
            LikesReceived = likesReceived
        };
    }
}
