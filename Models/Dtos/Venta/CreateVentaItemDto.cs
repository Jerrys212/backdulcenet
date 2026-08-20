namespace DulceAtardecer.Models.Dtos.Venta;

public record CreateVentaItemDto(
    int ProductoId,
    int Cantidad,
    List<int> ExtraIds
);
