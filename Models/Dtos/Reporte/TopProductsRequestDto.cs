namespace DulceAtardecer.Models.Dtos.Reporte;

public record TopProductsRequestDto(
    DateTime? StartDate,
    DateTime? EndDate,
    int? Limit
);
