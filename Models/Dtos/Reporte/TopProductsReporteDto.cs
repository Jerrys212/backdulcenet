namespace DulceAtardecer.Models.Dtos.Reporte;

public record TopProductsReporteDto(
    IEnumerable<ReportProductDto> TopProducts,
    IEnumerable<NotSoldProductDto> NotSoldProducts
);
