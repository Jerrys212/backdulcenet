namespace DulceAtardecer.Models.Dtos.SubCategoria;

public record SubCategoriaDto(
    int Id,
    string Nombre,
    int CategoriaId,
    string CategoriaNombre
);
