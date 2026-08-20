namespace DulceAtardecer.Models;

public class VentaItemExtra
{
    public int VentaItemId { get; set; }
    public VentaItem? VentaItem { get; set; }

    public int ExtraId { get; set; }
    public Extra? Extra { get; set; }

    /// <summary>Precio del extra congelado al momento de la venta (igual que VentaItem.Precio con el producto).</summary>
    public decimal Precio { get; set; }
}
