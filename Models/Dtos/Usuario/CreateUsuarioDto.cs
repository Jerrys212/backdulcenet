namespace DulceAtardecer.Models.Dtos.Usuario;

public record CreateUsuarioDto(
    string Username,
    string Email,
    string Password,
    string Nombre,
    string Role
);
