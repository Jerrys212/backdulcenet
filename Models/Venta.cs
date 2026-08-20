namespace DulceAtardecer.Models;

public class Venta
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime EstadoActualizadoEn { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public string SellerId { get; set; } = string.Empty;
    public ApplicationUser? Seller { get; set; }

    public string EstadoActualizadoPorId { get; set; } = string.Empty;
    public ApplicationUser? EstadoActualizadoPor { get; set; }

    public ICollection<VentaItem> Items { get; set; } = new List<VentaItem>();
}
