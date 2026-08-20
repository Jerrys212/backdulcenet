namespace DulceAtardecer.Models.Dtos.Producto;

public record ProductoDto(
    int Id,
    string Nombre,
    string Descripcion,
    decimal Precio,
    string ImgUrl,
    bool Activo,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    int CategoriaId,
    string CategoriaNombre,
    int SubCategoriaId,
    string SubCategoriaNombre
);
