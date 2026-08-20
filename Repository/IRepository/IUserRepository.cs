using DulceAtardecer.Models.Dtos.Auth;

namespace DulceAtardecer.Repository.IRepository;

public interface IUserRepository
{
    Task<bool> IsUniqueUserAsync(string username, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);
}
