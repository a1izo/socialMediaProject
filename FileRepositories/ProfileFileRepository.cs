using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class ProfileFileRepository : IProfileRepository
{
    private readonly string profilesFilePath = "profiles.json";
    private readonly string modulesFilePath = "profileModules.json";

    public ProfileFileRepository()
    {
        JsonFileHelper.EnsureFileExists(profilesFilePath);
        JsonFileHelper.EnsureFileExists(modulesFilePath);
    }

    public async Task<Profile> AddAsync(Profile profile)
    {
        List<Profile> profiles = await JsonFileHelper.LoadAsync<Profile>(profilesFilePath);
        if (profiles.Any(p => p.UserId == profile.UserId))
        {
            throw new InvalidOperationException(
                $"User '{profile.UserId}' already has a profile");
        }

        profile.Id = profiles.Any() ? profiles.Max(p => p.Id) + 1 : 1;
        profiles.Add(profile);
        await JsonFileHelper.SaveAsync(profilesFilePath, profiles);
        return profile;
    }

    public async Task UpdateAsync(Profile profile)
    {
        List<Profile> profiles = await JsonFileHelper.LoadAsync<Profile>(profilesFilePath);
        Profile? existingProfile = profiles.SingleOrDefault(p => p.Id == profile.Id);
        if (existingProfile is null)
        {
            throw new NotFoundException(
                $"Profile with ID '{profile.Id}' not found");
        }

        profiles.Remove(existingProfile);
        profiles.Add(profile);
        await JsonFileHelper.SaveAsync(profilesFilePath, profiles);
    }

    public async Task DeleteAsync(int id)
    {
        List<Profile> profiles = await JsonFileHelper.LoadAsync<Profile>(profilesFilePath);
        Profile? profileToRemove = profiles.SingleOrDefault(p => p.Id == id);
        if (profileToRemove is null)
        {
            throw new NotFoundException(
                $"Profile with ID '{id}' not found");
        }

        profiles.Remove(profileToRemove);
        await JsonFileHelper.SaveAsync(profilesFilePath, profiles);
        await ReplaceModulesAsync(profileToRemove.UserId, new List<ProfileModule>());
    }

    public async Task<Profile> GetSingleAsync(int id)
    {
        List<Profile> profiles = await JsonFileHelper.LoadAsync<Profile>(profilesFilePath);
        Profile? profile = profiles.SingleOrDefault(p => p.Id == id);
        if (profile is null)
        {
            throw new NotFoundException(
                $"Profile with ID '{id}' not found");
        }

        return profile;
    }

    public IQueryable<Profile> GetMany()
    {
        List<Profile> profiles = JsonFileHelper.Load<Profile>(profilesFilePath);
        return profiles.AsQueryable();
    }

    public IQueryable<ProfileModule> GetModules(int userId)
    {
        List<ProfileModule> modules = JsonFileHelper.Load<ProfileModule>(modulesFilePath);
        return modules.Where(m => m.UserId == userId).OrderBy(m => m.Order).AsQueryable();
    }

    public async Task ReplaceModulesAsync(int userId, List<ProfileModule> modules)
    {
        List<ProfileModule> allModules = await JsonFileHelper.LoadAsync<ProfileModule>(modulesFilePath);
        allModules.RemoveAll(m => m.UserId == userId);

        int nextId = allModules.Any() ? allModules.Max(m => m.Id) + 1 : 1;
        foreach (ProfileModule module in modules)
        {
            module.Id = nextId++;
            module.UserId = userId;
            allModules.Add(module);
        }

        await JsonFileHelper.SaveAsync(modulesFilePath, allModules);
    }
}
