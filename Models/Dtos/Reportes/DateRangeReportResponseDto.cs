namespace DulceAtardecer.Models.Dtos.Reportes;

public record DateRangeReportResponseDto(
    IReadOnlyList<ReportProductDto> TopProducts,
    IReadOnlyList<ReportProductDto> LeastSoldProducts
);
