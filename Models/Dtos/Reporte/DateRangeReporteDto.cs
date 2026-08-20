namespace DulceAtardecer.Models.Dtos.Reporte;

public record DateRangeReporteDto(
    IEnumerable<ReportProductDto> TopProducts,
    IEnumerable<ReportProductDto> LeastSoldProducts
);
