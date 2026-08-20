namespace DulceAtardecer.Models.Dtos.Reporte;

public record UserPerformanceDto(
    string UserId,
    string Username,
    IEnumerable<DailySalesEntryDto> DailySales,
    DailySalesEntryDto? BestDay,
    int DaysWithSales,
    decimal AveragePerSale
);
