using ApiContracts;

namespace BlazorApp.Services;

public interface IProfileService
{
    public Task<ProfileDto> GetProfileAsync(string userName);
    public Task<List<ProfileCardDto>> GetCardsAsync(IEnumerable<int> userIds);
    public Task UpdateProfileAsync(int userId, UpdateProfileDto request);
}
