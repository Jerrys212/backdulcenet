namespace DulceAtardecer.Models.Dtos.Reportes;

public record DailyReportResponseDto(
    string Date,
    decimal TotalAmount,
    IReadOnlyList<ReportProductDto> TopProducts
);
