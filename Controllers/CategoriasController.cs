using Asp.Versioning;
using DulceAtardecer.Common.Responses;
using DulceAtardecer.Constants;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Categoria;
using DulceAtardecer.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController(ICategoriaRepository categoriaRepository) : ControllerBase
{
    [HttpGet]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoriaDto>>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        (IEnumerable<Categoria> items, int total) = await categoriaRepository.GetAllAsync(page, limit, cancellationToken);
        IEnumerable<CategoriaDto> dtos = items.Adapt<IEnumerable<CategoriaDto>>();
        return Ok(new ApiResponse<IEnumerable<CategoriaDto>>(true, dtos, new ApiMeta(page, total)));
    }

    [HttpGet("{id:int}")]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Categoria categoria = await categoriaRepository.GetByIdAsync(id, cancellationToken);
        return Ok(categoria.Adapt<CategoriaDto>());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoriaDto>> CreateAsync(CreateCategoriaDto createDto, CancellationToken cancellationToken)
    {
        Categoria categoria = createDto.Adapt<Categoria>();
        categoria = await categoriaRepository.CreateAsync(categoria, cancellationToken);
        CategoriaDto result = categoria.Adapt<CategoriaDto>();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, UpdateCategoriaDto updateDto, CancellationToken cancellationToken)
    {
        Categoria categoria = updateDto.Adapt<Categoria>();
        await categoriaRepository.UpdateAsync(id, categoria, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await categoriaRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
