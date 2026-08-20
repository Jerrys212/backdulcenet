namespace DulceAtardecer.Models.Dtos.Extra;

public record ExtraDto(
    int Id,
    string Nombre,
    decimal Precio,
    bool Activo,
    DateTime FechaCreacion,
    DateTime FechaActualizacion
);
