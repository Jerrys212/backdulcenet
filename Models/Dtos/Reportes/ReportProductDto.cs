namespace DulceAtardecer.Models.Dtos.Reportes;

public record ReportProductDto(
    string Id,
    string Name,
    string? Category,
    int QuantitySold,
    decimal TotalAmount
);
