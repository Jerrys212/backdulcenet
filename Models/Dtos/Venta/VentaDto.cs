namespace DulceAtardecer.Models.Dtos.Venta;

public record VentaDto(
    int Id,
    string Cliente,
    decimal Total,
    string Estado,
    DateTime EstadoActualizadoEn,
    string EstadoActualizadoPorNombre,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    string SellerId,
    string SellerNombre,
    IEnumerable<VentaItemDto> Items
);
