namespace DulceAtardecer.Models.Dtos.Reportes;

public record OptionalDateRangeDto(
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
