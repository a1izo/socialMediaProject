using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using WebAPI.Services;

namespace WebAPI.Controllers;

// Keyword search over accounts (user name, display name, tagline), posts (title, body, tags) and tags.
[ApiController]
[Route("[controller]")]
public class SearchController : ControllerBase
{
    private const int Limit = 30;

    private readonly IUserRepository userRepo;
    private readonly IProfileRepository profileRepo;
    private readonly IPostRepository postRepo;
    private readonly ITagRepository tagRepo;
    private readonly PostDtoBuilder postDtoBuilder;
    private readonly ProfileCardBuilder cardBuilder;

    public SearchController(IUserRepository userRepo, IProfileRepository profileRepo, IPostRepository postRepo,
        ITagRepository tagRepo, PostDtoBuilder postDtoBuilder, ProfileCardBuilder cardBuilder)
    {
        this.userRepo = userRepo;
        this.profileRepo = profileRepo;
        this.postRepo = postRepo;
        this.tagRepo = tagRepo;
        this.postDtoBuilder = postDtoBuilder;
        this.cardBuilder = cardBuilder;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search([FromQuery] string? q, [FromQuery] string? type)
    {
        string[] words = (q ?? "").ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => w.TrimStart('#', '@'))
            .Where(w => w.Length > 0)
            .ToArray();
        string kind = (type ?? "all").ToLower();
        SearchResultDto result = new();
        if (words.Length == 0)
        {
            return Ok(result);
        }

        bool Matches(params string?[] fields) =>
            words.All(w => fields.Any(f => f is not null && f.ToLower().Contains(w)));

        if (kind is "all" or "accounts")
        {
            Dictionary<int, Profile> profiles = profileRepo.GetMany().ToDictionary(p => p.UserId);
            List<User> users = userRepo.GetMany().ToList()
                .Where(u =>
                {
                    profiles.TryGetValue(u.Id, out Profile? profile);
                    return Matches(u.UserName, profile?.DisplayName, profile?.Tagline);
                })
                .Take(Limit)
                .ToList();
            foreach (User user in users)
            {
                result.Users.Add(await cardBuilder.BuildCardAsync(user));
            }
        }

        if (kind is "all" or "posts")
        {
            Dictionary<int, string> userNames = postDtoBuilder.GetUserNames();
            List<Post> posts = postRepo.GetMany().ToList()
                .Where(p => Matches(p.Title, p.Body,
                    string.Join(" ", postRepo.GetTagsForPost(p.Id).Select(t => t.Name)),
                    PostDtoBuilder.UserNameOf(p.UserId, userNames)))
                .OrderByDescending(p => p.CreatedDate)
                .Take(Limit)
                .ToList();
            foreach (Post post in posts)
            {
                result.Posts.Add(await postDtoBuilder.BuildAsync(post, userNames));
            }
        }

        if (kind is "all" or "tags")
        {
            result.Tags = tagRepo.GetMany().ToList()
                .Where(t => words.Any(w => t.Name.ToLower().Contains(w)))
                .Take(Limit)
                .Select(DtoMapper.ToDto)
                .ToList();
        }

        return Ok(result);
    }
}
