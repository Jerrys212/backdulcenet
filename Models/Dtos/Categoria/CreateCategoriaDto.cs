using System.ComponentModel.DataAnnotations;

namespace DulceAtardecer.Models.Dtos.Categoria;

public record CreateCategoriaDto(
    [Required, MaxLength(100)] string Nombre
);
