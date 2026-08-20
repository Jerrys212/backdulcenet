namespace DulceAtardecer.Models.Dtos.Reporte;

public record DateRangeRequestDto(
    DateTime? StartDate,
    DateTime? EndDate
);
