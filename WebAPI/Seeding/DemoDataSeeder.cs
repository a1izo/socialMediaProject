using System.Text.Json;
using ApiContracts;
using Entities;
using RepositoryContracts;
using WebAPI.Profiles;

namespace WebAPI.Seeding;

// Fills an empty data folder with a small, believable community so the app looks alive on first run:
// 18 people with their own looks, about a month of posts, replies, likes, follows, chats and notifications.
// Every demo user has the password "cybernet". The first user (nyx) is a moderator.
// To reseed, stop the API and delete the .json data files in the WebAPI folder.
public static class DemoDataSeeder
{
    public const string DemoPassword = "cybernet";

    private record PostSpec(string Key, string Author, double DaysAgo, string Title, string Body, string[] Tags, string[] Media);

    // ReplyTo: the user whose latest reply on the same post this one answers
    private record CommentSpec(string PostKey, string Author, string Body, string? ReplyTo = null);

    public static async Task SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        IUserRepository userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IPostRepository postRepo = scope.ServiceProvider.GetRequiredService<IPostRepository>();
        ICommentRepository commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepository>();
        ITagRepository tagRepo = scope.ServiceProvider.GetRequiredService<ITagRepository>();
        IProfileRepository profileRepo = scope.ServiceProvider.GetRequiredService<IProfileRepository>();
        IMessageRepository messageRepo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        INotificationRepository notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        if (userRepo.GetMany().Any())
        {
            return;
        }

        DateTime now = DateTime.Now;
        Random random = new(42);

        // ------------------------------------------------------------------ people

        (string UserName, bool Mod, int DaysAgo)[] people =
        {
            ("nyx", true, 64), ("paperghost", false, 61), ("cobalt_exe", false, 58), ("hana_dots", false, 55),
            ("k1ra", false, 51), ("moss", false, 47), ("lumen_wav", false, 44), ("tidepool", false, 41),
            ("quiet_cartographer", false, 39), ("glitter_goblin", false, 36), ("ottoline", false, 33),
            ("ramen_rider", false, 30), ("vesper_moth", false, 28), ("pixelpilot", false, 25), ("soft_static", false, 22),
            ("bytebaker", false, 19), ("neon_noodle", false, 15), ("rainy_reader", false, 11)
        };

        Dictionary<string, User> u = new();
        foreach ((string userName, bool mod, int daysAgo) in people)
        {
            u[userName] = await userRepo.AddAsync(new User
            {
                UserName = userName,
                Password = DemoPassword,
                Email = $"{userName}@cybernet.test",
                IsModerator = mod,
                MessagePrivacy = userName is "vesper_moth" or "rainy_reader" ? "following" : "requests",
                CreatedDate = now.AddDays(-daysAgo)
            });
        }

        // ------------------------------------------------------------------ tags

        Dictionary<string, Tag> tags = new();
        foreach (string name in new[]
                 {
                     "art", "dither", "y2k", "sigil", "music", "dev", "meta", "type", "photo", "film", "ocean", "maps",
                     "gamedev", "pixel", "poetry", "books", "baking", "collage", "zine", "plants", "synth", "night", "help"
                 })
        {
            tags[name] = await tagRepo.AddAsync(new Tag { Name = name });
        }

        // ------------------------------------------------------------------ posts (oldest first)

        PostSpec[] postSpecs =
        {
            new("welcome", "nyx", 30.2, "welcome to cybernet",
                "this is a network where **your profile is yours**. move every module, recolor everything, break the grid if you want.\n\nhouse rules: be kind, be weird, credit other people's work.", new[] { "meta" }, new string[0]),
            new("paper1", "paperghost", 29.6, "slow internet, careful hands",
                "i like profiles that feel like paper. grain, black ink, thin lines. nothing blinking at me.", new[] { "type", "art" }, new string[0]),
            new("dither", "hana_dots", 29.1, "dithering is love expressed in dots",
                "spent the whole night turning photos into 1-bit halftones. there's something nice about throwing information away on purpose.", new[] { "dither", "art" }, new[] { "/demo/art-halftone.svg" }),
            new("sigils", "cobalt_exe", 28.4, "drew 40 sigils this week",
                "thin lines, sharp points, a bit of chrome. the best ones are in my gallery module.", new[] { "sigil", "art" }, new string[0]),
            new("stack", "nyx", 27.8, "how this place is built",
                "blazor front end, a rest web api, json files for storage. every profile module is its own component. ask me anything.", new[] { "dev", "meta" }, new string[0]),
            new("condensed", "k1ra", 27.0, "condensed type or nothing",
                "if your headline isn't screaming in a 90pt condensed face, is it even a headline?", new[] { "type" }, new[] { "/demo/art-specimen.svg" }),
            new("lumen1", "lumen_wav", 26.3, "",
                "finished the first track for the night bus ep. headphones on, lights off.", new[] { "music", "synth" }, new[] { "/demo/art-waveform.svg" }),
            new("tide1", "tidepool", 25.7, "low tide at 6am",
                "the anemones close up when you get too close. the hermit crabs do not care at all.", new[] { "ocean", "photo" }, new[] { "/demo/art-tidepool.svg" }),
            new("map1", "quiet_cartographer", 25.0, "a map of my walk to the bakery",
                "contour lines are made up. the bakery is real. the red line is the long way round because of the dog on 4th street.", new[] { "maps", "art" }, new[] { "/demo/art-map.svg" }),
            new("bloom", "moss", 24.4, "soft gradients for soft days",
                "made my profile look like a blurry photo of a garden at golden hour. recommend the bloom look if you need a calmer page.", new[] { "art" }, new string[0]),
            new("goblin1", "glitter_goblin", 23.9, "made my page as pink as legally allowed",
                "chrome stars, a checkerboard and the rgb split on max. my eyes hurt and i am happy.", new[] { "y2k" }, new[] { "/demo/art-chrome.svg" }),
            new("film1", "ottoline", 23.2, "portra 400, a cheap lens and a lot of luck",
                "first roll back from the lab in months. half of it is blurry and i love the blurry ones most.", new[] { "film", "photo" }, new[] { "/demo/art-sunset.svg", "/demo/art-city.svg" }),
            new("level1", "ramen_rider", 22.6, "devlog #1: the jump feels right now",
                "spent two days on coyote time and jump buffering. nobody will notice, everyone will feel it.", new[] { "gamedev", "pixel" }, new[] { "/demo/art-level.svg" }),
            new("vesper1", "vesper_moth", 22.0, "",
                "the moth doesn't want the flame.\nit wants the moon\nand settles for the porch light.", new[] { "poetry", "night" }, new string[0]),
            new("pip1", "pixelpilot", 21.4, "16x16 cat, 6 colors",
                "palette is limited on purpose. the pink nose was the hardest decision of my week.", new[] { "pixel", "art" }, new[] { "/demo/art-pixelcat.svg" }),
            new("collage1", "soft_static", 20.8, "zine #3 is cut and pasted",
                "magazines from the free shelf, a glue stick that's mostly dried out, one very long evening.", new[] { "collage", "zine" }, new[] { "/demo/art-collage.svg" }),
            new("bread1", "bytebaker", 20.1, "sourdough day",
                "the starter is 4 years old. the bread is 4 hours old. the backend i was supposed to finish is 0% done.", new[] { "baking" }, new[] { "/demo/art-bread.svg" }),
            new("playlist", "cobalt_exe", 19.5, "what do you listen to while you code?",
                "drop it in the replies. i'll add the best ones to the embed module on my profile.", new[] { "music", "dev" }, new string[0]),
            new("noodle1", "neon_noodle", 18.9, "",
                "night shift at the shop. the sign buzzes louder than the fridge. 2am regulars are the best regulars.", new[] { "night" }, new[] { "/demo/art-neonsign.svg" }),
            new("mira1", "rainy_reader", 18.2, "currently reading pile",
                "seven books, three started, zero finished. this is fine.", new[] { "books" }, new[] { "/demo/art-books.svg" }),
            new("css", "nyx", 17.6, "tip: the custom css box",
                "advanced settings let you write your own css. it only applies inside your profile, so go wild. you can't break anyone else's page.", new[] { "meta", "dev" }, new string[0]),
            new("y2k", "hana_dots", 17.0, "y2k chrome is back (it never left)",
                "four-point stars, chrome gradients, tiny monospace labels. the year 2000 knew what it was doing.", new[] { "y2k", "sigil" }, new string[0]),
            new("tide2", "tidepool", 16.4, "exam week",
                "if anyone needs me i'll be memorizing the life cycle of a sea star. please send snacks or memes about echinoderms.", new[] { "ocean" }, new string[0]),
            new("break", "k1ra", 15.9, "BREAK THE FEED",
                "red. white. black. nothing else.", new[] { "type" }, new string[0]),
            new("garden", "moss", 15.3, "pixel flowers",
                "dithered a flower into 2500 little squares. it's in my gallery.", new[] { "dither", "art" }, new[] { "/demo/pixel-bloom.svg" }),
            new("map2", "quiet_cartographer", 14.8, "",
                "asking for help: what do you call the little arrow that points north? i've been calling it \"the pointy\" for years.", new[] { "maps", "help" }, new string[0]),
            new("lumen2", "lumen_wav", 14.2, "shared my synth patch",
                "two oscillators, one detuned by 7 cents, a slow filter sweep. it sounds like a rainy bus window. the settings are in my info module.", new[] { "synth", "music" }, new string[0]),
            new("vesper2", "vesper_moth", 13.7, "small hours",
                "*three in the morning is a country\nwith no flag\nand very good soup.*", new[] { "poetry", "night" }, new[] { "/demo/art-moons.svg" }),
            new("film2", "ottoline", 13.1, "",
                "developed my own black and white for the first time. the bathroom smells like vinegar and i'm a little proud.", new[] { "film" }, new string[0]),
            new("goblin2", "glitter_goblin", 12.6, "ranked my favorite loading screens",
                "1. anything with a spinning chrome logo\n2. the ones with tips that don't help\n3. elevator music menus\n\nno notes.", new[] { "y2k" }, new string[0]),
            new("level2", "ramen_rider", 12.0, "devlog #2: coins",
                "added coins. then removed them. then added them back with a sound that's 40ms shorter. game design is fake and i am its servant.", new[] { "gamedev" }, new string[0]),
            new("pip2", "pixelpilot", 11.5, "",
                "what palette size do you all work with? i'm stuck between 8 and 16 for my next piece.", new[] { "pixel", "help" }, new string[0]),
            new("bread2", "bytebaker", 10.9, "deployed and baked",
                "pushed the fix, the build went green, the oven timer went off at the same second. best day of the month.", new[] { "dev", "baking" }, new[] { "/demo/art-code.svg" }),
            new("paper2", "paperghost", 10.4, "",
                "a page that loads in under a second is a kind of kindness.", new[] { "meta" }, new string[0]),
            new("collage2", "soft_static", 9.8, "trade zines with me?",
                "i have 12 copies of #3 left. send me yours and i'll send mine. anywhere is fine, i like stamps.", new[] { "zine" }, new string[0]),
            new("mush", "moss", 9.3, "found these on a walk",
                "drew them from memory when i got home, so they're probably not a real species. don't eat drawings.", new[] { "art", "plants" }, new[] { "/demo/art-mushrooms.svg" }),
            new("noodle2", "neon_noodle", 8.7, "new sign for the shop",
                "the old one only said \"OPE\". people liked it. the owner did not.", new[] { "night" }, new string[0]),
            new("mira2", "rainy_reader", 8.2, "",
                "finished one! it was the shortest one. still counts.", new[] { "books" }, new string[0]),
            new("cobalt2", "cobalt_exe", 7.6, "",
                "new sigils from this week", new[] { "sigil", "art" }, new[] { "/demo/sigil-star.svg", "/demo/orbit.svg", "/demo/hud-ring.svg" }),
            new("tide3", "tidepool", 7.1, "passed!!",
                "sea star exam: survived. celebrating by looking at sea stars.", new[] { "ocean" }, new string[0]),
            new("k1ra3", "k1ra", 6.5, "hot take",
                "rounded corners are a crime against the grid. fight me in the replies (respectfully).", new[] { "type" }, new string[0]),
            new("plant", "soft_static", 6.0, "",
                "my only plant has lived for a whole year. i drew her as a thank you.", new[] { "plants", "art" }, new[] { "/demo/art-plant.svg" }),
            new("lumen3", "lumen_wav", 5.4, "night bus ep is out",
                "four tracks, nineteen minutes, best with headphones and a window seat. link is in my embed module.", new[] { "music", "synth" }, new string[0]),
            new("map3", "quiet_cartographer", 4.9, "",
                "update: it's called a compass rose. you all were very nice about it. i'm still calling it the pointy.", new[] { "maps" }, new string[0]),
            new("goblin3", "glitter_goblin", 4.4, "",
                "if your profile doesn't have at least one sparkle on it i assume you're a cop", new[] { "y2k" }, new string[0]),
            new("vesper3", "vesper_moth", 3.9, "",
                "the rain sounds like someone typing a very long apology.", new[] { "poetry" }, new string[0]),
            new("level3", "ramen_rider", 3.3, "devlog #3: the first level is done",
                "it takes 90 seconds to beat and took me 3 weeks to make. playtesters said it's \"fine\" which is the nicest thing anyone has said to me.", new[] { "gamedev", "pixel" }, new string[0]),
            new("bread3", "bytebaker", 2.8, "",
                "anyone in the replies know why my api returns 415? the bread is fine, the content-type is not.", new[] { "dev", "help" }, new string[0]),
            new("film3", "ottoline", 2.2, "city at night, roll #12",
                "pushed two stops. grainy on purpose. the windows look like little pixels.", new[] { "film", "photo", "night" }, new[] { "/demo/art-city.svg" }),
            new("mira3", "rainy_reader", 1.7, "book club?",
                "thinking of starting a small one here. one book a month, we talk about it in the replies. who's in?", new[] { "books" }, new string[0]),
            new("noodle3", "neon_noodle", 1.2, "",
                "someone left a drawing of the shop on the counter. i'm framing it.", new[] { "night", "art" }, new string[0]),
            new("pip3", "pixelpilot", 0.8, "went with 8 colors",
                "thanks for all the opinions on my palette question. 8 it is. constraints are a gift.", new[] { "pixel" }, new string[0]),
            new("nyx3", "nyx", 0.5, "small update",
                "you can now search posts, people and tags, and there are notifications. tell me what breaks.", new[] { "meta", "dev" }, new string[0]),
            new("hana3", "hana_dots", 0.2, "",
                "printed 30 halftone postcards today. the ink is still drying on every surface of my room.", new[] { "dither", "art" }, new string[0])
        };

        Dictionary<string, Post> posts = new();
        foreach (PostSpec spec in postSpecs)
        {
            Post post = await postRepo.AddAsync(new Post
            {
                Title = spec.Title,
                Body = spec.Body,
                UserId = u[spec.Author].Id,
                CreatedDate = now.AddHours(-spec.DaysAgo * 24)
            });
            foreach (string tagName in spec.Tags)
            {
                await postRepo.AddTagAsync(post.Id, tags[tagName].Id);
            }

            for (int i = 0; i < spec.Media.Length; i++)
            {
                await postRepo.AddMediaAsync(new PostMedia { PostId = post.Id, Url = spec.Media[i], Kind = "image", Order = i });
            }

            posts[spec.Key] = post;
        }

        // ------------------------------------------------------------------ follows

        (string Follower, string[] Following)[] followSpecs =
        {
            ("nyx", new[] { "hana_dots", "cobalt_exe", "k1ra", "lumen_wav", "bytebaker", "pixelpilot" }),
            ("paperghost", new[] { "moss", "quiet_cartographer", "ottoline", "rainy_reader", "soft_static" }),
            ("cobalt_exe", new[] { "hana_dots", "lumen_wav", "glitter_goblin", "neon_noodle" }),
            ("hana_dots", new[] { "cobalt_exe", "moss", "soft_static", "pixelpilot", "ottoline" }),
            ("k1ra", new[] { "cobalt_exe", "soft_static", "glitter_goblin" }),
            ("moss", new[] { "hana_dots", "paperghost", "tidepool", "rainy_reader" }),
            ("lumen_wav", new[] { "neon_noodle", "cobalt_exe", "vesper_moth", "ramen_rider" }),
            ("tidepool", new[] { "ottoline", "moss", "quiet_cartographer" }),
            ("quiet_cartographer", new[] { "paperghost", "tidepool", "bytebaker" }),
            ("glitter_goblin", new[] { "k1ra", "pixelpilot", "ramen_rider", "neon_noodle" }),
            ("ottoline", new[] { "tidepool", "neon_noodle", "vesper_moth", "hana_dots" }),
            ("ramen_rider", new[] { "pixelpilot", "lumen_wav", "bytebaker" }),
            ("vesper_moth", new[] { "rainy_reader", "ottoline", "paperghost" }),
            ("pixelpilot", new[] { "ramen_rider", "glitter_goblin", "hana_dots" }),
            ("soft_static", new[] { "hana_dots", "k1ra", "paperghost" }),
            ("bytebaker", new[] { "quiet_cartographer", "ramen_rider" }),
            ("neon_noodle", new[] { "lumen_wav", "ottoline" }),
            ("rainy_reader", new[] { "vesper_moth", "paperghost", "moss" })
        };

        List<(User Follower, User Following, DateTime When)> follows = new();
        foreach ((string follower, string[] following) in followSpecs)
        {
            foreach (string target in following.Append("nyx").Distinct().Where(t => t != follower))
            {
                await userRepo.FollowAsync(u[follower].Id, u[target].Id);
                DateTime when = Later(u[follower].CreatedDate, u[target].CreatedDate).AddHours(random.Next(2, 200));
                follows.Add((u[follower], u[target], when > now ? now.AddHours(-random.Next(1, 30)) : when));
            }
        }

        HashSet<(int, int)> followPairs = follows.Select(f => (f.Follower.Id, f.Following.Id)).ToHashSet();

        // ------------------------------------------------------------------ replies

        CommentSpec[] commentSpecs =
        {
            new("welcome", "paperghost", "finally a place where my page can look like a zine"),
            new("welcome", "nyx", "that's exactly the point", "paperghost"),
            new("welcome", "k1ra", "FIRST"),
            new("welcome", "moss", "it's so calm here. i like it."),
            new("dither", "cobalt_exe", "teach me your ways"),
            new("dither", "hana_dots", "floyd-steinberg and patience", "cobalt_exe"),
            new("dither", "pixelpilot", "the dot sizes on this are so even. do you do it by hand?"),
            new("dither", "hana_dots", "half by hand, half by script. don't tell anyone", "pixelpilot"),
            new("sigils", "moss", "the chrome ones are unreal"),
            new("stack", "paperghost", "do modules load their own data?"),
            new("stack", "nyx", "yes, each one asks the api for what it needs", "paperghost"),
            new("stack", "bytebaker", "json files as a database is the most honest thing i've heard all week"),
            new("condensed", "hana_dots", "anton supremacy"),
            new("condensed", "paperghost", "respectfully: a nice serif at 11pt is also screaming, just quietly"),
            new("condensed", "k1ra", "that's not screaming that's whispering", "paperghost"),
            new("lumen1", "neon_noodle", "playing this in the shop tonight"),
            new("lumen1", "lumen_wav", "best place it could possibly be played", "neon_noodle"),
            new("tide1", "ottoline", "this is so pretty. what time is low tide where you are?"),
            new("tide1", "tidepool", "it moves every day! 6:10 today", "ottoline"),
            new("map1", "rainy_reader", "the dog detour is the best part"),
            new("map1", "tidepool", "i would read a whole atlas of your walks"),
            new("goblin1", "k1ra", "this is an attack"),
            new("goblin1", "glitter_goblin", "thank you", "k1ra"),
            new("goblin1", "pixelpilot", "the chrome stars are so crisp"),
            new("film1", "tidepool", "the blurry ones ARE the best ones"),
            new("film1", "vesper_moth", "the sunset one feels like a memory"),
            new("level1", "pixelpilot", "coyote time is love"),
            new("level1", "glitter_goblin", "can i playtest. i'm very bad at platformers, which makes me a great tester"),
            new("level1", "ramen_rider", "you're hired", "glitter_goblin"),
            new("vesper1", "rainy_reader", "oh this one stayed with me"),
            new("vesper1", "ottoline", "porch light gang"),
            new("pip1", "glitter_goblin", "the nose was the right call"),
            new("pip1", "ramen_rider", "can this cat be in my game"),
            new("pip1", "pixelpilot", "only if it gets a hat", "ramen_rider"),
            new("collage1", "paperghost", "dried out glue sticks are part of the medium"),
            new("collage1", "k1ra", "the type on the cover is so good"),
            new("bread1", "nyx", "the backend can wait. the bread can't."),
            new("bread1", "quiet_cartographer", "what's the starter called"),
            new("bread1", "bytebaker", "gerald", "quiet_cartographer"),
            new("playlist", "nyx", "lots of ambient with no lyrics"),
            new("playlist", "moss", "rain sounds and lo-fi"),
            new("playlist", "lumen_wav", "i'm biased but: synth stuff with no drums"),
            new("playlist", "ramen_rider", "video game soundtracks, the calm menu music ones"),
            new("playlist", "cobalt_exe", "adding all of these", "ramen_rider"),
            new("noodle1", "lumen_wav", "2am regulars are a real community"),
            new("noodle1", "ottoline", "can i come take photos of the sign some night?"),
            new("noodle1", "neon_noodle", "yes!! tuesdays are quiet", "ottoline"),
            new("mira1", "vesper_moth", "three started is a respectable number"),
            new("mira1", "paperghost", "what's on top of the pile?"),
            new("mira1", "rainy_reader", "a very long novel about a lighthouse", "paperghost"),
            new("css", "glitter_goblin", "i am going to do terrible things with this"),
            new("css", "nyx", "that's what it's for", "glitter_goblin"),
            new("css", "cobalt_exe", "is there a size limit?"),
            new("css", "nyx", "20k characters. if you need more than that, i want to see it", "cobalt_exe"),
            new("tide2", "moss", "sending you a snack in spirit 🍙"),
            new("tide2", "quiet_cartographer", "sea stars can regrow arms. you can regrow your sleep schedule after exams."),
            new("break", "soft_static", "the restraint is the point"),
            new("break", "glitter_goblin", "needs more pink"),
            new("break", "k1ra", "NO", "glitter_goblin"),
            new("map2", "tidepool", "compass rose!"),
            new("map2", "paperghost", "it's a compass rose, but \"the pointy\" is better"),
            new("map2", "bytebaker", "the pointy is canon now"),
            new("lumen2", "cobalt_exe", "rainy bus window is exactly what it sounds like"),
            new("vesper2", "rainy_reader", "very good soup"),
            new("vesper2", "neon_noodle", "the soup is real, come by at 3"),
            new("film2", "hana_dots", "vinegar smell = success"),
            new("goblin2", "ramen_rider", "taking notes for my game's loading screen"),
            new("level2", "pixelpilot", "40ms matters. i believe you."),
            new("pip2", "hana_dots", "8. limits make you braver."),
            new("pip2", "ramen_rider", "16, but use only 8 of them"),
            new("pip2", "glitter_goblin", "as many pinks as possible"),
            new("bread2", "nyx", "this is the dream"),
            new("collage2", "hana_dots", "trading you a halftone postcard set"),
            new("collage2", "soft_static", "deal!! check your messages", "hana_dots"),
            new("mush", "paperghost", "\"don't eat drawings\" is good advice in general"),
            new("noodle2", "rainy_reader", "petition to bring back OPE"),
            new("noodle2", "lumen_wav", "OPE forever"),
            new("mira2", "moss", "short books count double"),
            new("cobalt2", "hana_dots", "the orbit one!!"),
            new("tide3", "ottoline", "congrats!!"),
            new("tide3", "moss", "proud of you, sea star"),
            new("k1ra3", "moss", "but they're so soft"),
            new("k1ra3", "k1ra", "soft is a choice. a wrong one.", "moss"),
            new("k1ra3", "glitter_goblin", "my entire profile is rounded corners and i have never been happier"),
            new("lumen3", "neon_noodle", "on repeat in the shop already"),
            new("lumen3", "cobalt_exe", "added to my embed module"),
            new("lumen3", "vesper_moth", "track 3 at midnight is something else"),
            new("map3", "tidepool", "the pointy lives on"),
            new("level3", "glitter_goblin", "beat it in 4 minutes. it's great. i'm bad."),
            new("level3", "ramen_rider", "4 minutes is a valid speedrun category", "glitter_goblin"),
            new("bread3", "nyx", "you're probably sending form data. set Content-Type: application/json"),
            new("bread3", "bytebaker", "that was it. thank you. i owe you bread", "nyx"),
            new("bread3", "nyx", "i accept payment in bread", "bytebaker"),
            new("film3", "neon_noodle", "is that our street??"),
            new("film3", "ottoline", "it is! the blue window is your shop", "neon_noodle"),
            new("mira3", "vesper_moth", "in, if we can read poetry sometimes"),
            new("mira3", "paperghost", "in"),
            new("mira3", "moss", "yes please"),
            new("mira3", "rainy_reader", "ok, doing it. first pick next week", "moss"),
            new("noodle3", "ottoline", "it might have been me"),
            new("pip3", "ramen_rider", "constraints are a gift and also a curse"),
            new("nyx3", "k1ra", "search works. notifications work. i'm suspicious."),
            new("nyx3", "bytebaker", "no 415s here")
        };

        List<Comment> comments = new();
        foreach (CommentSpec spec in commentSpecs)
        {
            Post post = posts[spec.PostKey];
            Comment? parent = spec.ReplyTo is null
                ? null
                : comments.Last(c => c.PostId == post.Id && c.UserId == u[spec.ReplyTo].Id);
            DateTime after = parent?.CreatedDate ?? post.CreatedDate;
            DateTime created = after.AddMinutes(random.Next(8, 300));
            if (created > now)
            {
                created = now.AddMinutes(-random.Next(1, 20));
            }

            comments.Add(await commentRepo.AddAsync(new Comment
            {
                Body = spec.Body, UserId = u[spec.Author].Id, PostId = post.Id, ParentCommentId = parent?.Id, CreatedDate = created
            }));
        }

        // ------------------------------------------------------------------ likes
        // people mostly like posts from people they follow, and sometimes from strangers

        List<User> everyone = u.Values.ToList();
        List<(User Liker, Post Post, DateTime When)> postLikes = new();
        foreach (Post post in posts.Values)
        {
            foreach (User liker in everyone.Where(x => x.Id != post.UserId && x.CreatedDate < post.CreatedDate))
            {
                double chance = followPairs.Contains((liker.Id, post.UserId)) ? 0.55 : 0.12;
                if (random.NextDouble() < chance)
                {
                    await postRepo.LikeAsync(post.Id, liker.Id);
                    DateTime when = post.CreatedDate.AddMinutes(random.Next(5, 1800));
                    postLikes.Add((liker, post, when > now ? now.AddMinutes(-random.Next(1, 30)) : when));
                }
            }
        }

        List<(User Liker, Comment Comment, DateTime When)> commentLikes = new();
        foreach (Comment comment in comments)
        {
            foreach (User liker in everyone.Where(x => x.Id != comment.UserId))
            {
                if (random.NextDouble() < 0.07)
                {
                    await commentRepo.LikeAsync(comment.Id, liker.Id);
                    DateTime when = comment.CreatedDate.AddMinutes(random.Next(5, 600));
                    commentLikes.Add((liker, comment, when > now ? now.AddMinutes(-random.Next(1, 30)) : when));
                }
            }
        }

        // ------------------------------------------------------------------ notifications for the last week

        DateTime cutoff = now.AddDays(-7);
        List<Notification> notifications = new();
        notifications.AddRange(postLikes.Where(l => l.When > cutoff).Select(l => new Notification
        {
            UserId = l.Post.UserId, ActorId = l.Liker.Id, Type = "like", PostId = l.Post.Id, CreatedDate = l.When
        }));
        notifications.AddRange(commentLikes.Where(l => l.When > cutoff).Select(l => new Notification
        {
            UserId = l.Comment.UserId, ActorId = l.Liker.Id, Type = "commentLike", PostId = l.Comment.PostId, CommentId = l.Comment.Id, CreatedDate = l.When
        }));
        foreach (Comment comment in comments.Where(c => c.CreatedDate > cutoff))
        {
            Comment? parent = comment.ParentCommentId is null ? null : comments.First(c => c.Id == comment.ParentCommentId);
            int receiver = parent?.UserId ?? posts.Values.First(p => p.Id == comment.PostId).UserId;
            if (receiver != comment.UserId)
            {
                notifications.Add(new Notification
                {
                    UserId = receiver, ActorId = comment.UserId, Type = parent is null ? "comment" : "reply",
                    PostId = comment.PostId, CommentId = comment.Id, CreatedDate = comment.CreatedDate
                });
            }
        }

        notifications.AddRange(follows.Where(f => f.When > cutoff).Select(f => new Notification
        {
            UserId = f.Following.Id, ActorId = f.Follower.Id, Type = "follow", CreatedDate = f.When
        }));

        foreach (Notification notification in notifications.OrderBy(n => n.CreatedDate))
        {
            // anything older than a day has been seen
            notification.IsRead = notification.CreatedDate < now.AddDays(-1);
            await notificationRepo.AddAsync(notification);
        }

        // ------------------------------------------------------------------ messages

        async Task Chat(string a, string b, bool accepted, params (string From, string Text, double HoursAgo)[] lines)
        {
            DateTime first = now.AddHours(-lines.Max(l => l.HoursAgo));
            DateTime last = now.AddHours(-lines.Min(l => l.HoursAgo));
            Conversation conversation = await messageRepo.AddConversationAsync(new Conversation
            {
                UserOneId = u[a].Id, UserTwoId = u[b].Id, StartedById = u[a].Id, IsAccepted = accepted,
                CreatedDate = first, LastMessageDate = last,
                // the starter has read everything; the other side has read all but the newest few hours
                UserOneLastRead = last, UserTwoLastRead = accepted ? last.AddHours(-2) : null
            });
            foreach ((string from, string text, double hoursAgo) in lines)
            {
                await messageRepo.AddMessageAsync(new Message
                {
                    ConversationId = conversation.Id, SenderId = u[from].Id, Body = text, CreatedDate = now.AddHours(-hoursAgo)
                });
            }
        }

        await Chat("nyx", "hana_dots", true,
            ("nyx", "your halftone post is so good", 50), ("hana_dots", "thank you!! took forever", 49.5),
            ("nyx", "could you make one for my banner?", 6), ("hana_dots", "yes, send me a photo", 1));
        await Chat("lumen_wav", "neon_noodle", true,
            ("lumen_wav", "heard you played the ep in the shop", 30), ("neon_noodle", "every night this week. people ask what it is", 29),
            ("lumen_wav", "that's the best thing anyone's told me", 28.5), ("neon_noodle", "free noodles if you come play live", 3));
        await Chat("ottoline", "tidepool", true,
            ("ottoline", "can i come to low tide with you and bring a camera?", 70), ("tidepool", "yes! saturday, 6am, bring boots", 68),
            ("ottoline", "6am. ok. for art.", 67.8));
        await Chat("ramen_rider", "pixelpilot", true,
            ("ramen_rider", "serious question: would you draw the enemies for my game", 44), ("pixelpilot", "with 8 colors?", 43),
            ("ramen_rider", "with 8 colors", 42.9), ("pixelpilot", "i'm in. send me the list", 42));
        await Chat("soft_static", "hana_dots", true,
            ("soft_static", "zine trade! what's your address?", 20), ("hana_dots", "sending it now, and the postcards are ready", 19));
        await Chat("glitter_goblin", "k1ra", false,
            ("glitter_goblin", "hi i know you hate pink but would you design a logo for my stream", 5));
        await Chat("bytebaker", "nyx", false,
            ("bytebaker", "thanks again for the 415 fix. the bread offer is real", 2));

        // ------------------------------------------------------------------ profiles

        await SaveProfile(profileRepo, u["nyx"], "nyx", "moderator of the void // builds things at 3am", "/demo/sigil-star.svg",
            "/demo/banner-void.svg", ThemePresets.Create("void"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 12, null,
                    new { Markdown = "# you are now entering the void", Align = "left", Size = "xl" }),
                ProfileDefaults.Module(ProfileModuleTypes.Status, 4, "currently",
                    new { Emoji = "✦", Label = "status", Text = "fixing whatever you reported" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 8, "about",
                    new { Markdown = "i run this place. i like **dithered images**, ambient music and *very* dark themes.\n\n- moderator\n- builder\n- night owl" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 4, "stats", new { Layout = "tiles" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "posts", new { Count = 4, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "archive",
                    new { Images = new[] { "/demo/sigil-star.svg", "/demo/pixel-bloom.svg", "/demo/orbit.svg", "/demo/hud-ring.svg" }, Columns = 2, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 6, "top friends",
                    new { UserIds = new[] { u["hana_dots"].Id, u["cobalt_exe"].Id, u["bytebaker"].Id, u["k1ra"].Id } }),
                ProfileDefaults.Module(ProfileModuleTypes.Divider, 12, null, new { Style = "stars", Height = 48 }),
                ProfileDefaults.Module(ProfileModuleTypes.TagCloud, 6, "frequencies", new { Count = 10 }),
                ProfileDefaults.Module(ProfileModuleTypes.Followers, 6, "followers", new { Count = 12 })
            }, stickers: new List<StickerDto>
            {
                new() { Kind = "glyph", Value = "star", X = 88, Y = 300, Size = 90, Rotation = 12, Color = "#ff2a4f" },
                new() { Kind = "glyph", Value = "sparkle", X = 4, Y = 640, Size = 56, Rotation = -8 },
                new() { Kind = "emoji", Value = "🕯️", X = 93, Y = 980, Size = 48, Rotation = 6 }
            });

        await SaveProfile(profileRepo, u["paperghost"], "paper ghost", "slow internet, careful hands", null, "/demo/banner-paper.svg",
            ThemePresets.Create("paper"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 8, null,
                    new { Markdown = "## nothing blinking.\nonly grain, ink and thin lines.", Align = "left", Size = "lg" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 4, "index",
                    new { Fields = new[] { new { Key = "based in", Value = "a quiet room" }, new { Key = "medium", Value = "ink, paper, css" }, new { Key = "pronouns", Value = "they/them" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "notes", new { Count = 3, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 4, "plate 01",
                    new { Images = new[] { "/demo/hud-ring.svg" }, Columns = 1, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Followers, 12, "readers", new { Count = 12 })
            });

        await SaveProfile(profileRepo, u["cobalt_exe"], "cobalt.exe", "sigils, chrome and electric blue", "/demo/orbit.svg",
            "/demo/banner-sigil.svg", ThemePresets.Create("sigil"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 6, "now playing",
                    new { Emoji = "♫", Label = "listening", Text = "night bus ep, on loop" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 6, null, new { Layout = "inline" }),
                ProfileDefaults.Module(ProfileModuleTypes.PinnedPosts, 6, "pinned", new { PostIds = new[] { posts["sigils"].Id, posts["playlist"].Id } }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "sigil archive",
                    new { Images = new[] { "/demo/orbit.svg", "/demo/sigil-star.svg", "/demo/hud-ring.svg", "/demo/art-chrome.svg" }, Columns = 2, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Links, 12, "links",
                    new { Links = new[] { new { Label = "portfolio", Url = "https://example.com", Icon = "star" }, new { Label = "mixes", Url = "https://example.com", Icon = "music" } } })
            }, ".banner { filter: saturate(1.3); }\nh2 { letter-spacing: 0.08em; }");

        await SaveProfile(profileRepo, u["hana_dots"], "hana", "every image is a dot pattern if you squint", "/demo/halftone-sphere.svg", null,
            ThemePresets.Create("halftone"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 4, "dots",
                    new { Images = new[] { "/demo/halftone-sphere.svg", "/demo/art-halftone.svg" }, Columns = 1, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 8, "hi",
                    new { Markdown = "print maker. i turn everything into **halftone**.\n\n> losing information on purpose is a design choice.\n\npostcards are open, message me." }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "posts", new { Count = 4, ShowExcerpt = false }),
                ProfileDefaults.Module(ProfileModuleTypes.TagCloud, 4, "tags", new { Count = 8 })
            });

        await SaveProfile(profileRepo, u["k1ra"], "K1RA", "loud type. louder opinions.", null, null,
            ThemePresets.Create("signal"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 12, null,
                    new { Markdown = "# BREAK THE FEED", Align = "center", Size = "xl" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 4, "numbers", new { Layout = "tiles" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "posts", new { Count = 5, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Divider, 12, null, new { Style = "ticks", Height = 40 }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 12, "crew", new { UserIds = new[] { u["nyx"].Id, u["cobalt_exe"].Id, u["soft_static"].Id } })
            });

        await SaveProfile(profileRepo, u["moss"], "moss", "you don't have to be ready", "/demo/pixel-bloom.svg", null,
            ThemePresets.Create("bloom"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 12, null,
                    new { Emoji = "✿", Label = "mood", Text = "growing slowly" }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "garden",
                    new { Images = new[] { "/demo/pixel-bloom.svg", "/demo/art-mushrooms.svg" }, Columns = 2, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 6, "about me",
                    new { Markdown = "soft gradients, pixel flowers, golden hour.\n\nthe *bloom* look is my home." }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "posts", new { Count = 3, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["lumen_wav"], "lumen", "synths, night buses, headphones", "/demo/av-lumen.svg", "/demo/bn-synth.svg",
            Look("synth", "classic", nameStyle: "neon", decoration: "orbit"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 4, "now", new { Emoji = "♫", Label = "making", Text = "track 5 (it's a secret)" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 8, "about",
                    new { Markdown = "i make **slow synth music** for late rides home.\n\nthe *night bus* ep is out now. four tracks, nineteen minutes." }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 6, "patch notes",
                    new { Fields = new[] { new { Key = "osc 1", Value = "saw, -12" }, new { Key = "osc 2", Value = "saw, +7 cents" }, new { Key = "filter", Value = "low pass, slow sweep" }, new { Key = "reverb", Value = "yes" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 6, "updates", new { Count = 3, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 12, "listeners", new { UserIds = new[] { u["neon_noodle"].Id, u["cobalt_exe"].Id, u["vesper_moth"].Id } })
            });

        await SaveProfile(profileRepo, u["tidepool"], "tess", "marine biology student. ask me about sea stars", "/demo/av-tess.svg", "/demo/bn-tide.svg",
            Look("aurora", "centered", decoration: "ring"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 12, "hi",
                    new { Markdown = "second year marine bio. i spend most mornings at low tide.\n\n- favorite animal: the ochre sea star\n- least favorite: exams" }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 12, "tide pools",
                    new { Images = new[] { "/demo/art-tidepool.svg", "/demo/art-sunset.svg" }, Columns = 2, Aspect = "natural" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "field notes", new { Count = 4, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["quiet_cartographer"], "atlas", "walking every street on the map", "/demo/av-atlas.svg", "/demo/bn-topo.svg",
            Look("zine", "magazine"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 8, null, new { Markdown = "## maps of places i actually walked", Align = "left", Size = "lg" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 4, "legend",
                    new { Fields = new[] { new { Key = "red line", Value = "the long way" }, new { Key = "dots", Value = "good bread" }, new { Key = "the pointy", Value = "north" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "sheets", new { Images = new[] { "/demo/art-map.svg" }, Columns = 1, Aspect = "natural" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 6, "notes", new { Count = 3, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["glitter_goblin"], "glitter goblin", "pink is a personality", "/demo/av-goblin.svg", "/demo/bn-goblin.svg",
            Look("chrome", "classic", nameStyle: "chrome", decoration: "spark", sparkles: true), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 12, null, new { Markdown = "# ✦ welcome to my page ✦", Align = "center", Size = "lg" }),
                ProfileDefaults.Module(ProfileModuleTypes.Status, 6, "playing", new { Emoji = "🎮", Label = "now", Text = "a platformer i'm bad at" }),
                ProfileDefaults.Module(ProfileModuleTypes.Stats, 6, null, new { Layout = "inline" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "posts", new { Count = 4, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 4, "besties", new { UserIds = new[] { u["pixelpilot"].Id, u["ramen_rider"].Id } })
            }, stickers: new List<StickerDto>
            {
                new() { Kind = "emoji", Value = "💖", X = 95, Y = 560, Size = 64, Rotation = 10 },
                new() { Kind = "glyph", Value = "sparkle", X = 6, Y = 420, Size = 60, Rotation = -6, Color = "#ff8fd8" }
            });

        await SaveProfile(profileRepo, u["ottoline"], "otto", "35mm, mostly at night", "/demo/av-otto.svg", "/demo/bn-film.svg",
            Look("dither", "overlay"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 12, "contact sheet",
                    new { Images = new[] { "/demo/art-city.svg", "/demo/art-sunset.svg", "/demo/art-tidepool.svg" }, Columns = 3, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 4, "kit",
                    new { Fields = new[] { new { Key = "camera", Value = "an old slr from a flea market" }, new { Key = "film", Value = "portra 400, hp5" }, new { Key = "lens", Value = "50mm, slightly foggy" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 8, "rolls", new { Count = 3, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["ramen_rider"], "rio", "making a tiny platformer, one jump at a time", "/demo/av-rio.svg", "/demo/bn-pixel.svg",
            Look("arcade", "sidebar"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 12, "working on", new { Emoji = "👾", Label = "devlog", Text = "level 2: the cave" }),
                ProfileDefaults.Module(ProfileModuleTypes.PinnedPosts, 12, "devlogs", new { PostIds = new[] { posts["level1"].Id, posts["level2"].Id, posts["level3"].Id } }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 12, "screenshots", new { Images = new[] { "/demo/art-level.svg", "/demo/art-pixelcat.svg" }, Columns = 2, Aspect = "natural" })
            });

        await SaveProfile(profileRepo, u["vesper_moth"], "vesper", "short poems for the small hours", "/demo/av-vesper.svg", "/demo/bn-moth.svg",
            Look("blood moon", "centered", nameStyle: "typewriter", decoration: "aura"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Text, 12, null,
                    new { Markdown = "*i write when everyone else is asleep.*", Align = "center", Size = "lg" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "poems", new { Count = 5, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.Divider, 12, null, new { Style = "stars", Height = 40 })
            });

        await SaveProfile(profileRepo, u["pixelpilot"], "pip", "pixel art, small palettes", "/demo/av-pip.svg", "/demo/bn-pixel.svg",
            Look("arcade", "classic", decoration: "pixel"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 8, "pieces", new { Images = new[] { "/demo/art-pixelcat.svg", "/demo/art-level.svg" }, Columns = 2, Aspect = "square" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 4, "palette",
                    new { Fields = new[] { new { Key = "colors", Value = "8" }, new { Key = "canvas", Value = "16 to 64 px" }, new { Key = "commissions", Value = "ask me" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "posts", new { Count = 3, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["soft_static"], "ines", "zines, glue sticks, mail", "/demo/av-ines.svg", "/demo/bn-collage.svg",
            Look("zine", "classic"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 6, "about",
                    new { Markdown = "i make **collage zines** out of whatever the free shelf gives me.\n\ntrades are always open. i love stamps." }),
                ProfileDefaults.Module(ProfileModuleTypes.Gallery, 6, "issues", new { Images = new[] { "/demo/art-collage.svg", "/demo/art-plant.svg" }, Columns = 2, Aspect = "natural" }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "posts", new { Count = 4, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["bytebaker"], "sam", "backend dev. bread on weekends, and weekdays", "/demo/av-baker.svg", "/demo/bn-bakery.svg",
            Look("paper", "classic"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 6, "oven", new { Emoji = "🍞", Label = "baking", Text = "rye, 2 hours left" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 6, "stack",
                    new { Fields = new[] { new { Key = "work", Value = "c#, sql, too much json" }, new { Key = "starter", Value = "gerald, 4 years old" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "posts", new { Count = 4, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["neon_noodle"], "noodle", "night shift at a noodle shop", "/demo/av-noodle.svg", "/demo/bn-neon.svg",
            Look("synth", "magazine", nameStyle: "neon", decoration: "glow"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 6, "shift", new { Emoji = "🍜", Label = "open", Text = "until 4am" }),
                ProfileDefaults.Module(ProfileModuleTypes.Bio, 6, "the shop",
                    new { Markdown = "small shop, big pots. the sign used to say OPE.\n\nregulars get extra egg." }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "from the counter", new { Count = 3, ShowExcerpt = true })
            });

        await SaveProfile(profileRepo, u["rainy_reader"], "mira", "reading slowly, on purpose", "/demo/av-mira.svg", "/demo/bn-shelf.svg",
            Look("bloom", "classic", decoration: "double"), new List<ProfileModule>
            {
                ProfileDefaults.Module(ProfileModuleTypes.Status, 6, "reading", new { Emoji = "📖", Label = "now", Text = "a very long novel about a lighthouse" }),
                ProfileDefaults.Module(ProfileModuleTypes.Info, 6, "this year",
                    new { Fields = new[] { new { Key = "started", Value = "14" }, new { Key = "finished", Value = "6" }, new { Key = "abandoned", Value = "2, no regrets" } } }),
                ProfileDefaults.Module(ProfileModuleTypes.LatestPosts, 12, "posts", new { Count = 3, ShowExcerpt = true }),
                ProfileDefaults.Module(ProfileModuleTypes.TopFriends, 12, "book club", new { UserIds = new[] { u["vesper_moth"].Id, u["paperghost"].Id, u["moss"].Id } })
            });
    }

    private static DateTime Later(DateTime a, DateTime b) => a > b ? a : b;

    // a look plus a few personal touches
    private static ProfileThemeDto Look(string preset, string layout, string nameStyle = "plain", string decoration = "none", bool sparkles = false)
    {
        ProfileThemeDto theme = ThemePresets.Create(preset);
        theme.Layout = layout;
        theme.NameStyle = nameStyle;
        theme.AvatarDecoration = decoration;
        theme.Sparkles = theme.Sparkles || sparkles;
        ThemePresets.ApplyAvatarDefaults(theme);
        return theme;
    }

    private static async Task SaveProfile(IProfileRepository profileRepo, User user, string displayName, string tagline,
        string? avatarUrl, string? bannerUrl, ProfileThemeDto theme, List<ProfileModule> modules, string? customCss = null,
        List<StickerDto>? stickers = null)
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
            StickersJson = stickers is null ? null : JsonSerializer.Serialize(stickers),
            // spread out, so "recently restyled" changes over time
            UpdatedDate = DateTime.Now.AddHours(-(19 - user.Id) * 6)
        });

        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Order = i;
        }

        await profileRepo.ReplaceModulesAsync(user.Id, modules);
    }
}
