using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Models.Dtos.Producto;

public class CreateProductoDto
{
    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required, Range(0, double.MaxValue)]
    public decimal Precio { get; set; }

    [Required]
    public int CategoriaId { get; set; }

    [FromForm]
    public IFormFile? Imagen { get; set; }
}
