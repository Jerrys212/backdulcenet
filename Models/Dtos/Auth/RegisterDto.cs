namespace DulceAtardecer.Models.Dtos.Auth;

public record RegisterDto(
    string Username,
    string Email,
    string Password,
    string Nombre
);
