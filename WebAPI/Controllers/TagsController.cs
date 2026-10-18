using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagRepository tagRepo;

    public TagsController(ITagRepository tagRepo)
    {
        this.tagRepo = tagRepo;
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> AddTag([FromBody] CreateTagDto request)
    {
        DtoMapper.RequireNotBlank(request.Name, "Name");
        VerifyNameIsAvailable(request.Name);

        Tag created = await tagRepo.AddAsync(new Tag { Name = request.Name });
        TagDto dto = DtoMapper.ToDto(created);
        return Created($"/tags/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateTag([FromRoute] int id, [FromBody] UpdateTagDto request)
    {
        DtoMapper.RequireNotBlank(request.Name, "Name");

        Tag tag = await tagRepo.GetSingleAsync(id);
        VerifyNameIsAvailable(request.Name, id);
        tag.Name = request.Name;
        await tagRepo.UpdateAsync(tag);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TagDto>> GetSingleTag([FromRoute] int id)
    {
        Tag tag = await tagRepo.GetSingleAsync(id);
        return Ok(DtoMapper.ToDto(tag));
    }

    [HttpGet]
    public ActionResult<IEnumerable<TagDto>> GetManyTags([FromQuery] string? nameContains)
    {
        IQueryable<Tag> tags = tagRepo.GetMany();
        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            tags = tags.Where(t => t.Name.ToLower().Contains(nameContains.ToLower()));
        }

        List<TagDto> dtos = tags.OrderBy(t => t.Name).ToList().Select(DtoMapper.ToDto).ToList();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTag([FromRoute] int id)
    {
        await tagRepo.DeleteAsync(id);
        return NoContent();
    }

    private void VerifyNameIsAvailable(string name, int? excludeTagId = null)
    {
        bool taken = tagRepo.GetMany()
            .Any(t => t.Name.ToLower() == name.ToLower() && t.Id != excludeTagId);
        if (taken)
        {
            throw new InvalidOperationException($"Tag '{name}' already exists.");
        }
    }
}
