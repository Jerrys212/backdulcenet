using Asp.Versioning;
using DulceAtardecer.Common.Responses;
using DulceAtardecer.Models.Dtos.Usuario;
using DulceAtardecer.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UsuarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDto>>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        (IEnumerable<UsuarioDto> items, int total) = await userRepository.GetUsuariosAsync(page, limit, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<UsuarioDto>>(true, items, new ApiMeta(page, total)));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        UsuarioDto usuario = await userRepository.GetUsuarioByIdAsync(id, cancellationToken);
        return Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> CreateAsync(CreateUsuarioDto createDto, CancellationToken cancellationToken)
    {
        UsuarioDto result = await userRepository.CreateUsuarioAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync(string id, UpdateUsuarioDto updateDto, CancellationToken cancellationToken)
    {
        await userRepository.UpdateUsuarioAsync(id, updateDto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await userRepository.DeleteUsuarioAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPasswordAsync(string id, ResetPasswordDto resetDto, CancellationToken cancellationToken)
    {
        await userRepository.ResetPasswordAsync(id, resetDto.NewPassword, cancellationToken);
        return NoContent();
    }
}
