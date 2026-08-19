using Asp.Versioning;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.SubCategoria;
using DulceAtardecer.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize]
public class SubCategoriasController(ISubCategoriaRepository subCategoriaRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SubCategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SubCategoriaDto>>> GetAllAsync(
        [FromQuery] int? categoriaId, CancellationToken cancellationToken)
    {
        IEnumerable<SubCategoria> subCategorias = await subCategoriaRepository.GetAllAsync(categoriaId, cancellationToken);
        return Ok(subCategorias.Adapt<IEnumerable<SubCategoriaDto>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SubCategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubCategoriaDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        SubCategoria subCategoria = await subCategoriaRepository.GetByIdAsync(id, cancellationToken);
        return Ok(subCategoria.Adapt<SubCategoriaDto>());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SubCategoriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubCategoriaDto>> CreateAsync(CreateSubCategoriaDto createDto, CancellationToken cancellationToken)
    {
        SubCategoria subCategoria = createDto.Adapt<SubCategoria>();
        subCategoria = await subCategoriaRepository.CreateAsync(subCategoria, cancellationToken);
        SubCategoria subCategoriaCreada = await subCategoriaRepository.GetByIdAsync(subCategoria.Id, cancellationToken);
        SubCategoriaDto result = subCategoriaCreada.Adapt<SubCategoriaDto>();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, UpdateSubCategoriaDto updateDto, CancellationToken cancellationToken)
    {
        SubCategoria subCategoria = updateDto.Adapt<SubCategoria>();
        await subCategoriaRepository.UpdateAsync(id, subCategoria, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await subCategoriaRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
