using Entities;

namespace RepositoryContracts;

public interface IProfileRepository
{
    Task<Profile> AddAsync(Profile profile);
    Task UpdateAsync(Profile profile);
    Task DeleteAsync(int id);
    Task<Profile> GetSingleAsync(int id);
    IQueryable<Profile> GetMany();

    //module logic
    IQueryable<ProfileModule> GetModules(int userId);
    Task ReplaceModulesAsync(int userId, List<ProfileModule> modules);
}
