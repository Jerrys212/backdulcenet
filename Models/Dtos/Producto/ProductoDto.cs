namespace DulceAtardecer.Models.Dtos.Producto;

public record ProductoDto(
    int Id,
    string Nombre,
    string Descripcion,
    decimal Precio,
    string ImgUrl,
    int CategoriaId,
    string CategoriaNombre
);
