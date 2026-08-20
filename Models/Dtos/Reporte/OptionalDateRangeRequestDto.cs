namespace DulceAtardecer.Models.Dtos.Reporte;

public record OptionalDateRangeRequestDto(
    DateTime? StartDate,
    DateTime? EndDate
);
