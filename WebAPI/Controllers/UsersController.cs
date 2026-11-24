using ApiContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using WebAPI.Profiles;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;
    private static readonly string[] messagePrivacyOptions = { "everyone", "requests", "following", "nobody" };

    private readonly IProfileRepository profileRepo;
    private readonly Notifier notifier;

    public UsersController(IUserRepository userRepo, IProfileRepository profileRepo, Notifier notifier)
    {
        this.userRepo = userRepo;
        this.profileRepo = profileRepo;
        this.notifier = notifier;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        DtoMapper.RequireNotBlank(request.UserName, "User name");
        DtoMapper.RequireNotBlank(request.Password, "Password");
        DtoMapper.RequireNotBlank(request.Email, "Email");
        VerifyUserNameIsAvailable(request.UserName);
        VerifyEmailIsAvailable(request.Email);

        User user = new()
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber,
            IsModerator = false,
            CreatedDate = DateTime.Now,
            MessagePrivacy = "requests"
        };
        User created = await userRepo.AddAsync(user);
        await ProfileDefaults.CreateAsync(profileRepo, created);
        UserDto dto = DtoMapper.ToDto(created);
        return Created($"/users/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        DtoMapper.RequireNotBlank(request.UserName, "User name");
        DtoMapper.RequireNotBlank(request.Email, "Email");

        User user = await userRepo.GetSingleAsync(id);
        VerifyUserNameIsAvailable(request.UserName, id);
        VerifyEmailIsAvailable(request.Email, id);

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = request.Password;
        }

        if (request.MessagePrivacy is not null)
        {
            if (!messagePrivacyOptions.Contains(request.MessagePrivacy))
            {
                throw new ArgumentException($"Message privacy must be one of: {string.Join(", ", messagePrivacyOptions)}.");
            }

            user.MessagePrivacy = request.MessagePrivacy;
        }

        await userRepo.UpdateAsync(user);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetSingleUser([FromRoute] int id)
    {
        User user = await userRepo.GetSingleAsync(id);
        return Ok(DtoMapper.ToDto(user));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetManyUsers([FromQuery] string? userNameContains)
    {
        IQueryable<User> users = userRepo.GetMany();
        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            users = users.Where(u => u.UserName.ToLower().Contains(userNameContains.ToLower()));
        }

        List<User> result = await users.OrderBy(u => u.Id).ToListAsync();
        return Ok(result.Select(DtoMapper.ToDto).ToList());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        // the database removes everything that belongs to the user with it:
        // profile, modules, posts, comments, likes, follows, notifications and conversations
        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/followers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetFollowers([FromRoute] int id)
    {
        await userRepo.GetSingleAsync(id);
        List<User> followers = await userRepo.GetFollowers(id).OrderBy(u => u.UserName).ToListAsync();
        return Ok(followers.Select(DtoMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}/following")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetFollowing([FromRoute] int id)
    {
        await userRepo.GetSingleAsync(id);
        List<User> following = await userRepo.GetFollowing(id).OrderBy(u => u.UserName).ToListAsync();
        return Ok(following.Select(DtoMapper.ToDto).ToList());
    }

    [HttpPost("{id:int}/following")]
    public async Task<ActionResult> Follow([FromRoute] int id, [FromBody] FollowDto request)
    {
        if (id == request.FollowingId)
        {
            throw new ArgumentException("A user cannot follow themselves.");
        }

        await userRepo.GetSingleAsync(id);
        await userRepo.GetSingleAsync(request.FollowingId);
        await userRepo.FollowAsync(id, request.FollowingId);
        await notifier.NotifyAsync(request.FollowingId, id, Notifier.Follow);
        return NoContent();
    }

    [HttpDelete("{id:int}/following/{followingId:int}")]
    public async Task<ActionResult> Unfollow([FromRoute] int id, [FromRoute] int followingId)
    {
        await userRepo.UnfollowAsync(id, followingId);
        await notifier.RetractAsync(followingId, id, Notifier.Follow);
        return NoContent();
    }

    private void VerifyUserNameIsAvailable(string userName, int? excludeUserId = null)
    {
        bool taken = userRepo.GetMany()
            .Any(u => u.UserName.ToLower() == userName.ToLower() && u.Id != excludeUserId);
        if (taken)
        {
            throw new InvalidOperationException($"User name '{userName}' is already taken.");
        }
    }

    private void VerifyEmailIsAvailable(string email, int? excludeUserId = null)
    {
        bool taken = userRepo.GetMany()
            .Any(u => u.Email.ToLower() == email.ToLower() && u.Id != excludeUserId);
        if (taken)
        {
            throw new InvalidOperationException($"Email '{email}' is already in use.");
        }
    }
}
