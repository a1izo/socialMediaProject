using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IPostRepository postRepo;
    private readonly ITagRepository tagRepo;
    private readonly PostDtoBuilder postDtoBuilder;
    private readonly ProfileCardBuilder cardBuilder;

    public SearchController(IUserRepository userRepo, IPostRepository postRepo,
        ITagRepository tagRepo, PostDtoBuilder postDtoBuilder, ProfileCardBuilder cardBuilder)
    {
        this.userRepo = userRepo;
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

        // every word has to appear somewhere (case-insensitive); the filtering runs in the database
        if (kind is "all" or "accounts")
        {
            IQueryable<User> users = userRepo.GetMany();
            foreach (string word in words)
            {
                users = users.Where(u =>
                    u.UserName.ToLower().Contains(word) ||
                    (u.Profile != null && (u.Profile.DisplayName.ToLower().Contains(word) ||
                                           (u.Profile.Tagline != null && u.Profile.Tagline.ToLower().Contains(word)))));
            }

            foreach (User user in await users.OrderBy(u => u.Id).Take(Limit).ToListAsync())
            {
                result.Users.Add(await cardBuilder.BuildCardAsync(user));
            }
        }

        if (kind is "all" or "posts")
        {
            IQueryable<Post> posts = postRepo.GetMany();
            foreach (string word in words)
            {
                posts = posts.Where(p =>
                    p.Title.ToLower().Contains(word) ||
                    p.Body.ToLower().Contains(word) ||
                    p.User.UserName.ToLower().Contains(word) ||
                    p.PostTags.Any(pt => pt.Tag.Name.ToLower().Contains(word)));
            }

            result.Posts = await postDtoBuilder.BuildManyAsync(posts.OrderByDescending(p => p.CreatedDate).Take(Limit));
        }

        if (kind is "all" or "tags")
        {
            // any of the words is enough for a tag; the tag list is small, so this filters in memory
            List<Tag> allTags = await tagRepo.GetMany().OrderBy(t => t.Name).ToListAsync();
            result.Tags = allTags
                .Where(t => words.Any(w => t.Name.ToLower().Contains(w)))
                .Take(Limit)
                .Select(DtoMapper.ToDto)
                .ToList();
        }

        return Ok(result);
    }
}
