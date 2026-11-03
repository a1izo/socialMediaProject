using ApiContracts;

namespace BlazorApp.Services;

// Keeps the profile cards (avatar, display name, theme) of authors already seen during this
// session, so a feed only asks the API once per author.
public class ProfileCardCache
{
    private readonly IProfileService profileService;
    private readonly Dictionary<int, ProfileCardDto> cards = new();

    public ProfileCardCache(IProfileService profileService)
    {
        this.profileService = profileService;
    }

    public async Task EnsureAsync(IEnumerable<int> userIds)
    {
        List<int> missing = userIds.Distinct().Where(id => !cards.ContainsKey(id)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        foreach (ProfileCardDto card in await profileService.GetCardsAsync(missing))
        {
            cards[card.UserId] = card;
        }
    }

    public ProfileCardDto? Get(int userId)
    {
        return cards.GetValueOrDefault(userId);
    }

    public void Invalidate(int userId)
    {
        cards.Remove(userId);
    }
}
