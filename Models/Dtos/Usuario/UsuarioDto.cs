namespace DulceAtardecer.Models.Dtos.Usuario;

public record UsuarioDto(
    string Id,
    string Username,
    string Email,
    string Nombre,
    IList<string> Roles,
    bool IsActive
);
