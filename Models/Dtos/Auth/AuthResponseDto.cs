namespace DulceAtardecer.Models.Dtos.Auth;

public record AuthResponseDto(
    string Id,
    string Username,
    string Email,
    string Nombre,
    IList<string> Roles,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
