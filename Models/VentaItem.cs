namespace DulceAtardecer.Models;

public class VentaItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }

    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public ICollection<VentaItemExtra> Extras { get; set; } = new List<VentaItemExtra>();
}
