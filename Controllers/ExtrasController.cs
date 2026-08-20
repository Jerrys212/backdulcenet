using Asp.Versioning;
using DulceAtardecer.Common.Responses;
using DulceAtardecer.Constants;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Extra;
using DulceAtardecer.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize]
public class ExtrasController(IExtraRepository extraRepository) : ControllerBase
{
    [HttpGet]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExtraDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExtraDto>>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        (IEnumerable<Extra> items, int total) = await extraRepository.GetAllAsync(page, limit, cancellationToken);
        IEnumerable<ExtraDto> dtos = items.Adapt<IEnumerable<ExtraDto>>();
        return Ok(new ApiResponse<IEnumerable<ExtraDto>>(true, dtos, new ApiMeta(page, total)));
    }

    [HttpGet("{id:int}")]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(typeof(ExtraDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExtraDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Extra extra = await extraRepository.GetByIdAsync(id, cancellationToken);
        return Ok(extra.Adapt<ExtraDto>());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ExtraDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ExtraDto>> CreateAsync(CreateExtraDto createDto, CancellationToken cancellationToken)
    {
        Extra extra = createDto.Adapt<Extra>();
        extra = await extraRepository.CreateAsync(extra, cancellationToken);
        ExtraDto result = extra.Adapt<ExtraDto>();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, UpdateExtraDto updateDto, CancellationToken cancellationToken)
    {
        Extra extra = updateDto.Adapt<Extra>();
        await extraRepository.UpdateAsync(id, extra, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await extraRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
