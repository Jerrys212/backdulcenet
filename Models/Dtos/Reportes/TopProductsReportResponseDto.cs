namespace DulceAtardecer.Models.Dtos.Reportes;

public record TopProductsReportResponseDto(
    IReadOnlyList<ReportProductDto> TopProducts,
    IReadOnlyList<NotSoldProductDto> NotSoldProducts
);
