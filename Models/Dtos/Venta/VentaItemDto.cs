namespace DulceAtardecer.Models.Dtos.Venta;

public record VentaItemDto(
    int Id,
    int ProductoId,
    string Nombre,
    decimal Precio,
    int Cantidad,
    decimal Subtotal,
    IEnumerable<VentaItemExtraDto> Extras
);
