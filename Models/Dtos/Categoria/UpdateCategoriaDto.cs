using System.ComponentModel.DataAnnotations;

namespace DulceAtardecer.Models.Dtos.Categoria;

public record UpdateCategoriaDto(
    [Required, MaxLength(100)] string Nombre
);
