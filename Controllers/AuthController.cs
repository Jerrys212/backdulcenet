using Asp.Versioning;
using DulceAtardecer.Models.Dtos.Auth;
using DulceAtardecer.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
public class AuthController(IUserRepository userRepository) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (!await userRepository.IsUniqueUserAsync(registerDto.Username, cancellationToken))
        {
            return BadRequest("El nombre de usuario ya está en uso.");
        }

        AuthResponseDto response = await userRepository.RegisterAsync(registerDto, cancellationToken);
        return CreatedAtAction(nameof(RegisterAsync), response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        AuthResponseDto? response = await userRepository.LoginAsync(loginDto, cancellationToken);
        if (response is null)
        {
            return Unauthorized("Usuario o contraseña incorrectos.");
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshAsync(RefreshRequestDto refreshRequestDto, CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto response = await userRepository.RefreshAsync(refreshRequestDto.RefreshToken, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAsync(RefreshRequestDto revokeRequestDto, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst("id")?.Value;
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await userRepository.RevokeAsync(revokeRequestDto.RefreshToken, userId, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}
