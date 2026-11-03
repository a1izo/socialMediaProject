using System.Text.Json;
using ApiContracts;
using Entities;

namespace WebAPI;

internal static class DtoMapper
{
    public const string DeletedUserName = "[deleted]";

    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsModerator = user.IsModerator,
            CreatedDate = user.CreatedDate
        };
    }

    public static PostDto ToDto(Post post, string authorUserName, int likeCount)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            AuthorUserName = authorUserName,
            CreatedDate = post.CreatedDate,
            LikeCount = likeCount
        };
    }

    public static CommentDto ToDto(Comment comment, string authorUserName, int likeCount)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            UserId = comment.UserId,
            AuthorUserName = authorUserName,
            PostId = comment.PostId,
            ParentCommentId = comment.ParentCommentId,
            CreatedDate = comment.CreatedDate,
            LikeCount = likeCount
        };
    }

    public static TagDto ToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public static ProfileDto ToDto(Profile profile, User user, List<ProfileModule> modules, ProfileStatsDto stats)
    {
        return new ProfileDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = profile.DisplayName,
            Tagline = profile.Tagline,
            AvatarUrl = profile.AvatarUrl,
            BannerUrl = profile.BannerUrl,
            IsModerator = user.IsModerator,
            CreatedDate = user.CreatedDate,
            Theme = ReadTheme(profile.ThemeJson),
            CustomCss = profile.CustomCss,
            Modules = modules.OrderBy(m => m.Order).Select(ToDto).ToList(),
            Stats = stats
        };
    }

    public static ProfileCardDto ToCardDto(Profile profile, User user, ProfileStatsDto stats)
    {
        return new ProfileCardDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = profile.DisplayName,
            Tagline = profile.Tagline,
            AvatarUrl = profile.AvatarUrl,
            BannerUrl = profile.BannerUrl,
            IsModerator = user.IsModerator,
            Theme = ReadTheme(profile.ThemeJson),
            Stats = stats
        };
    }

    public static ProfileModuleDto ToDto(ProfileModule module)
    {
        return new ProfileModuleDto
        {
            Id = module.Id,
            Type = module.Type,
            Order = module.Order,
            Span = module.Span,
            Title = module.Title,
            ShowTitle = module.ShowTitle,
            IsVisible = module.IsVisible,
            SettingsJson = module.SettingsJson,
            Style = JsonSerializer.Deserialize<ModuleStyleDto>(module.StyleJson) ?? new ModuleStyleDto()
        };
    }

    public static ProfileThemeDto ReadTheme(string? themeJson)
    {
        if (string.IsNullOrWhiteSpace(themeJson))
        {
            return ThemePresets.Create(ThemePresets.Default);
        }

        return JsonSerializer.Deserialize<ProfileThemeDto>(themeJson) ?? ThemePresets.Create(ThemePresets.Default);
    }

    public static void RequireNotBlank(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }
    }
}
