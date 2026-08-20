namespace DulceAtardecer.Models.Dtos.Categoria;

public record CategoriaDto(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion,
    DateTime FechaActualizacion
);
