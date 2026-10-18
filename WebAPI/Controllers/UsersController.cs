using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
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
            CreatedDate = DateTime.Now
        };
        User created = await userRepo.AddAsync(user);
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
    public ActionResult<IEnumerable<UserDto>> GetManyUsers([FromQuery] string? userNameContains)
    {
        IQueryable<User> users = userRepo.GetMany();
        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            users = users.Where(u => u.UserName.ToLower().Contains(userNameContains.ToLower()));
        }

        List<UserDto> dtos = users.OrderBy(u => u.Id).ToList().Select(DtoMapper.ToDto).ToList();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/followers")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetFollowers([FromRoute] int id)
    {
        await userRepo.GetSingleAsync(id);
        List<UserDto> dtos = userRepo.GetFollowers(id).ToList().Select(DtoMapper.ToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:int}/following")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetFollowing([FromRoute] int id)
    {
        await userRepo.GetSingleAsync(id);
        List<UserDto> dtos = userRepo.GetFollowing(id).ToList().Select(DtoMapper.ToDto).ToList();
        return Ok(dtos);
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
        return NoContent();
    }

    [HttpDelete("{id:int}/following/{followingId:int}")]
    public async Task<ActionResult> Unfollow([FromRoute] int id, [FromRoute] int followingId)
    {
        await userRepo.UnfollowAsync(id, followingId);
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
