namespace DulceAtardecer.Models;

public class VentaItemExtra
{
    public int VentaItemId { get; set; }
    public VentaItem? VentaItem { get; set; }

    public int ExtraId { get; set; }
    public Extra? Extra { get; set; }
}
