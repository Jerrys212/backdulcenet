using DulceAtardecer.Models.Dtos.Auth;
using DulceAtardecer.Models.Dtos.Usuario;

namespace DulceAtardecer.Repository.IRepository;

public interface IUserRepository
{
    Task<bool> IsUniqueUserAsync(string username, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<UsuarioDto> Items, int Total)> GetUsuariosAsync(int page, int limit, CancellationToken cancellationToken = default);
    Task<UsuarioDto> GetUsuarioByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<UsuarioDto> CreateUsuarioAsync(CreateUsuarioDto createDto, CancellationToken cancellationToken = default);
    Task UpdateUsuarioAsync(string id, UpdateUsuarioDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteUsuarioAsync(string id, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string id, string newPassword, CancellationToken cancellationToken = default);
}
