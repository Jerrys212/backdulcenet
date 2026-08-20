namespace DulceAtardecer.Models.Dtos.Venta;

public record CreateVentaDto(
    string Cliente,
    List<CreateVentaItemDto> Items
);
