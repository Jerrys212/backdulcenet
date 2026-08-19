using Asp.Versioning;
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
[Authorize(Roles = "Admin")]
public class CategoriasController(ICategoriaRepository categoriaRepository) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        IEnumerable<Categoria> categorias = await categoriaRepository.GetAllAsync(cancellationToken);
        return Ok(categorias.Adapt<IEnumerable<CategoriaDto>>());
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Categoria categoria = await categoriaRepository.GetByIdAsync(id, cancellationToken);
        return Ok(categoria.Adapt<CategoriaDto>());
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoriaDto>> CreateAsync(CreateCategoriaDto createDto, CancellationToken cancellationToken)
    {
        Categoria categoria = createDto.Adapt<Categoria>();
        categoria = await categoriaRepository.CreateAsync(categoria, cancellationToken);
        CategoriaDto result = categoria.Adapt<CategoriaDto>();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, UpdateCategoriaDto updateDto, CancellationToken cancellationToken)
    {
        Categoria categoria = updateDto.Adapt<Categoria>();
        await categoriaRepository.UpdateAsync(id, categoria, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await categoriaRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
