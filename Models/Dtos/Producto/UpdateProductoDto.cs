using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Models.Dtos.Producto;

public class UpdateProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CategoriaId { get; set; }
    public int SubCategoriaId { get; set; }

    [FromForm]
    public IFormFile? Imagen { get; set; }
}
