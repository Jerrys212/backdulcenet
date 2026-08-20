namespace DulceAtardecer.Models.Dtos.Reportes;

public record UserPerformanceItemDto(
    string UserId,
    string Username,
    IReadOnlyList<DailySalesEntryDto> DailySales,
    DailySalesEntryDto? BestDay,
    int DaysWithSales,
    decimal AveragePerSale
);
