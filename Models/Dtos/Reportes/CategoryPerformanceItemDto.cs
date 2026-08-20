namespace DulceAtardecer.Models.Dtos.Reportes;

public record CategoryPerformanceItemDto(
    string CategoryId,
    string CategoryName,
    int ItemsSold,
    decimal Total,
    int UniqueProducts,
    int SalesCount,
    decimal AveragePerSale,
    decimal PercentOfTotalSales
);
