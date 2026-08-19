using DulceAtardecer.Models.Dtos.Auth;

namespace DulceAtardecer.Repository.IRepository;

public interface IUserRepository
{
    Task<bool> IsUniqueUserAsync(string username, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>Valida y rota un refresh token. Lanza UnauthorizedAccessException si no existe, expiró o ya fue revocado.</summary>
    Task<AuthResponseDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revoca un refresh token perteneciente al usuario indicado (logout). Lanza UnauthorizedAccessException si no existe o no le pertenece.</summary>
    Task RevokeAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);
}
