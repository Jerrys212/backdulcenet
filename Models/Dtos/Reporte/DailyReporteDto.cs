namespace DulceAtardecer.Models.Dtos.Reporte;

public record DailyReporteDto(
    string Date,
    decimal TotalAmount,
    IEnumerable<ReportProductDto> TopProducts
);
