using System.Text.Json;
using ApiContracts;
using Entities;
using RepositoryContracts;
using WebAPI.Profiles;

namespace WebAPI.Seeding;

// Fills an empty data folder with showcase users so the app looks alive on first run.
// Every demo user has the password "cybernet". The first user (nyx) is a moderator.
public static class DemoDataSeeder
{
    public const string DemoPassword = "cybernet";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        IUserRepository userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IPostRepository postRepo = scope.ServiceProvider.GetRequiredService<IPostRepository>();
        ICommentRepository commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepository>();
        ITagRepository tagRepo = scope.ServiceProvider.GetRequiredService<ITagRepository>();
        IProfileRepository profileRepo = scope.ServiceProvider.GetRequiredService<IProfileRepository>();

        if (userRepo.GetMany().Any())
        {
            return;
        }

        User nyx = await AddUser(userRepo, "nyx", true, 40);
        User paper = await AddUser(userRepo, "paperghost", false, 35);
        User cobalt = await AddUser(userRepo, "cobalt_exe", false, 30);
        User hana = await AddUser(userRepo, "hana_dots", false, 26);
        User kira = await AddUser(userRepo, "k1ra", false, 20);
        User moss = await AddUser(userRepo, "moss", false, 14);
        List<User> everyone = new() { nyx, paper, cobalt, hana, kira, moss };

        Dictionary<string, Tag> tags = new();
        foreach (string name in new[] { "dither", "y2k", "sigil", "music", "dev", "art", "meta", "type" })
        {
            tags[name] = await tagRepo.AddAsync(new Tag { Name = name });
        }

        async Task<Post> AddPost(User author, int daysAgo, string title, string body, params string[] tagNames)
        {
            Post post = await postRepo.AddAsync(new Post
            {
                Title = title, Body = body, UserId = author.Id, CreatedDate = DateTime.Now.AddDays(-daysAgo).AddHours(-daysAgo * 3)
            });
            foreach (string tagName in tagNames)
            {
                await postRepo.AddTagAsync(post.Id, tags[tagName].Id);
            }

            return post;
        }

        Post welcome = await AddPost(nyx, 13, "welcome to cybernet",
            "this is a network where **your profile is yours**. move every module, recolor everything, break the grid.\n\nrules: be kind, be weird, don't be boring.", "meta");
        Post dither = await AddPost(hana, 11, "dithering is just love expressed in dots",
            "spent the whole night converting photos into 1-bit halftones. there is something about losing information on purpose.", "dither", "art");
        Post sigils = await AddPost(cobalt, 10, "drew 40 sigils this week",
            "thin lines, sharp points, a little bit of chrome. posting the best ones in my gallery module.", "sigil", "art");
        Post stack = await AddPost(nyx, 9, "how this place is built",
            "Blazor front end, a REST Web API, JSON files for storage. every profile module is its own component. ask me anything.", "dev", "meta");
        Post type = await AddPost(kira, 8, "condensed type or nothing",
            "if your headline isn't screaming in a 90pt condensed face, is it even a headline?", "type", "y2k");
        Post bloom = await AddPost(moss, 7, "soft gradients for soft days",
            "made my profile look like a blurry photo of a garden at golden hour. highly recommend the bloom preset.", "art");
        Post paperPost = await AddPost(paper, 6, "slow internet, careful hands",
            "i like profiles that feel like paper. grain, black ink, thin lines. nothing blinking.", "type", "art");
        Post playlist = await AddPost(cobalt, 5, "what are you listening to while you code?",
            "drop it in the comments. i'll add the best ones to the embed module on my profile.", "music", "dev");
        Post y2k = await AddPost(hana, 4, "y2k chrome is back (it never left)",
            "four-point stars, chrome gradients, tiny monospace labels. the year 2000 was a vibe.", "y2k", "sigil");
        Post tip = await AddPost(nyx, 3, "tip: use the custom css tab",
            "advanced settings let you write your own css. it only ever applies inside your profile, so go wild.", "meta", "dev");
        Post signal = await AddPost(kira, 2, "BREAK THE FEED",
            "red. white. black. nothing else.", "type");
        Post garden = await AddPost(moss, 1, "pixel flowers",
            "dithered a flower into 2500 little squares. it's in my gallery.", "dither", "art");

        async Task<Comment> AddComment(User author, Post post, string body, Comment? parent = null)
        {
            return await commentRepo.AddAsync(new Comment
            {
                Body = body, UserId = author.Id, PostId = post.Id, ParentCommentId = parent?.Id,
                CreatedDate = post.CreatedDate.AddHours(2 + body.Length % 7)
            });
        }

        Comment c1 = await AddComment(paper, welcome, "finally a place where i can make my page look like a zine");
        await AddComment(nyx, welcome, "that's exactly the point", c1);
        await AddComment(kira, welcome, "first");
        Comment c2 = await AddComment(cobalt, dither, "teach me your ways");
        await AddComment(hana, dither, "floyd-steinberg and patience", c2);
        await AddComment(moss, sigils, "the chrome ones are unreal");
        Comment c3 = await AddComment(paper, stack, "do modules load their own data?");
        await AddComment(nyx, stack, "yes, each one asks the api for what it needs", c3);
        await AddComment(hana, type, "anton supremacy");
        await AddComment(nyx, playlist, "lots of ambient with no lyrics");
        await AddComment(moss, playlist, "rain sounds + lo-fi");
        await AddComment(cobalt, garden, "this is so good");

        // likes
        (Post post, User[] likers)[] likes =
        {
            (welcome, new[] { paper, cobalt, hana, kira, moss }),
            (dither, new[] { nyx, cobalt, moss, paper }),
            (sigils, new[] { nyx, hana, moss }),
            (stack, new[] { paper, kira }),
            (type, new[] { hana, nyx, cobalt }),
            (bloom, new[] { paper, hana }),
            (paperPost, new[] { moss, nyx }),
            (playlist, new[] { nyx, hana, kira, moss }),
            (y2k, new[] { cobalt, kira }),
            (signal, new[] { nyx, hana, cobalt }),
            (garden, new[] { hana, paper, cobalt })
        };
        foreach ((Post post, User[] likers) in likes)
        {
            foreach (User liker in likers)
            {
                await postRepo.LikeAsync(post.Id, liker.Id);
            }
        }

        // follows: everyone follows nyx, plus a few pairs
        foreach (User user in everyone.Where(u => u.Id != nyx.Id))
        {
            await userRepo.FollowAsync(user.Id, nyx.Id);
        }

        (User follower, User following)[] follows =
        {
            (nyx, hana), (nyx, cobalt), (nyx, kira), (hana, cobalt), (cobalt, hana),
            (moss, hana), (paper, moss), (kira, cobalt), (hana, moss), (moss, paper)
        };
        foreach ((User follower, User following) in follows)
        {
            await userRepo.FollowAsync(follower.Id, following.Id);
        }

        // profiles
        await SaveProfile(profileRepo, nyx, "nyx", "moderator of the void // builds things at 3am", "/demo/sigil-star.svg",
            "/demo/banner-void.svg", ThemePresets.Create("void"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 12, null,
                    new { Markdown = "# you are now entering the void", Align = "left", Size = "xl" }),
                ProfileDefaults.Module(ProfileModuleTypes.Status, 4, "currently",
                    new { Emoji = "✦", Label = "status", Text = "debugging the network" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 8, "about",
                    new { Markdown = "i run this place. i like **dithered images**, ambient music and *very* dark themes.\n\n- moderator\n- builder\n- night owl" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 4, "signal", new { Layout = "tiles" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "transmissions", new { Count = 4, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "archive",
                    new { Images = new[] { "/demo/sigil-star.svg", "/demo/pixel-bloom.svg", "/demo/orbit.svg", "/demo/hud-ring.svg" }, Columns = 2, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 6, "top friends",
                    new { UserIds = new[] { hana.Id, cobalt.Id, kira.Id } }),
                ProfileDefaults.Module(ProfileModuleTypes.Divider, 12, null, new { Style = "stars", Height = 48 }),
                ProfileDefaults.Module(ProfileModuleTypes.Links, 4, "elsewhere",
                    new { Links = new[] { new { Label = "github", Url = "https://github.com", Icon = "code" }, new { Label = "my site", Url = "https://example.com", Icon = "globe" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.TagCloud, 4, "frequencies", new { Count = 10 }),
                ProfileDefaults.Module(ProfileModuleTypes.Followers, 4, "followers", new { Count = 12 })
            });

        ProfileThemeDto paperTheme = ThemePresets.Create("paper");
        await SaveProfile(profileRepo, paper, "paper ghost", "slow internet, careful hands", null, "/demo/banner-paper.svg",
            paperTheme, new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 8, null,
                    new { Markdown = "## nothing blinking.\nonly grain, ink and thin lines.", Align = "left", Size = "lg" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 4, "index",
                    new { Fields = new[] { new { Key = "based in", Value = "a quiet room" }, new { Key = "medium", Value = "ink, paper, css" }, new { Key = "pronouns", Value = "they/them" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 4, "plate 01",
                    new { Images = new[] { "/demo/hud-ring.svg" }, Columns = 1, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "notes", new { Count = 3, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Followers, 12, "readers", new { Count = 12 })
            });

        await SaveProfile(profileRepo, cobalt, "cobalt.exe", "sigils, chrome and electric blue", "/demo/orbit.svg",
            "/demo/banner-sigil.svg", ThemePresets.Create("sigil"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 6, "now playing",
                    new { Emoji = "♫", Label = "listening", Text = "synth tracks on repeat" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 6, null, new { Layout = "inline" }),
                ProfileDefaults.Module(ProfileModuleTypes.PinnedPosts, 6, "pinned", new { PostIds = new[] { sigils.Id, playlist.Id } }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "sigil archive",
                    new { Images = new[] { "/demo/orbit.svg", "/demo/sigil-star.svg" }, Columns = 2, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Links, 12, "links",
                    new { Links = new[] { new { Label = "portfolio", Url = "https://example.com", Icon = "star" }, new { Label = "mixes", Url = "https://example.com", Icon = "music" } } })
            }, ".banner { filter: saturate(1.3); }\nh2 { letter-spacing: 0.08em; }");

        await SaveProfile(profileRepo, hana, "hana", "every image is a dot pattern if you squint", "/demo/halftone-sphere.svg", null,
            ThemePresets.Create("halftone"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 4, "dots",
                    new { Images = new[] { "/demo/halftone-sphere.svg" }, Columns = 1, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 8, "hi",
                    new { Markdown = "print maker. i convert everything into **halftone**.\n\n> losing information on purpose is a design choice." }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "posts", new { Count = 4, ShowExcerpt = false }),
                ProfileDefaults.Module(ProfileModuleTypes.TagCloud, 4, "tags", new { Count = 8 })
            });

        await SaveProfile(profileRepo, kira, "K1RA", "loud type. louder opinions.", null, null,
            ThemePresets.Create("signal"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 12, null,
                    new { Markdown = "# BREAK THE FEED", Align = "center", Size = "xl" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 4, "numbers", new { Layout = "tiles" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "posts", new { Count = 5, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Divider, 12, null, new { Style = "ticks", Height = 40 }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 12, "crew", new { UserIds = new[] { nyx.Id, cobalt.Id } })
            });

        await SaveProfile(profileRepo, moss, "moss", "you don't have to be ready", "/demo/pixel-bloom.svg", null,
            ThemePresets.Create("bloom"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 12, null,
                    new { Emoji = "✿", Label = "mood", Text = "growing slowly" }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "garden",
                    new { Images = new[] { "/demo/pixel-bloom.svg" }, Columns = 1, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 6, "about me",
                    new { Markdown = "soft gradients, pixel flowers, golden hour.\n\nthe *bloom* preset is my home." }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "posts", new { Count = 3, ShowExcerpt = true })
            });
    }

    private static async Task<User> AddUser(IUserRepository userRepo, string userName, bool isModerator, int daysAgo)
    {
        return await userRepo.AddAsync(new User
        {
            UserName = userName,
            Password = DemoPassword,
            Email = $"{userName}@cybernet.test",
            IsModerator = isModerator,
            CreatedDate = DateTime.Now.AddDays(-daysAgo)
        });
    }

    private static async Task SaveProfile(IProfileRepository profileRepo, User user, string displayName, string tagline,
        string? avatarUrl, string? bannerUrl, ProfileThemeDto theme, List<ProfileModule> modules, string? customCss = null)
    {
        await profileRepo.AddAsync(new Profile
        {
            UserId = user.Id,
            DisplayName = displayName,
            Tagline = tagline,
            AvatarUrl = avatarUrl,
            BannerUrl = bannerUrl,
            ThemeJson = JsonSerializer.Serialize(theme),
            CustomCss = customCss,
            UpdatedDate = DateTime.Now
        });

        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Order = i;
        }

        await profileRepo.ReplaceModulesAsync(user.Id, modules);
    }
}
