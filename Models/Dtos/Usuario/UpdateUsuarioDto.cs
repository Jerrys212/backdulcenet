namespace DulceAtardecer.Models.Dtos.Usuario;

public record UpdateUsuarioDto(
    string Nombre,
    string Email,
    string Role,
    bool IsActive
);
