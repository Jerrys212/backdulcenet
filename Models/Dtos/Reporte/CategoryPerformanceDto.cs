namespace DulceAtardecer.Models.Dtos.Reporte;

public record CategoryPerformanceDto(
    int CategoryId,
    string CategoryName,
    int ItemsSold,
    decimal Total,
    int UniqueProducts,
    int SalesCount,
    decimal AveragePerSale,
    decimal PercentOfTotalSales
);
