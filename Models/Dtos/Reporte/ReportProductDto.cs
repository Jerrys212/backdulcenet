namespace DulceAtardecer.Models.Dtos.Reporte;

public record ReportProductDto(
    int Id,
    string Name,
    string? Category,
    int QuantitySold,
    decimal TotalAmount
);
