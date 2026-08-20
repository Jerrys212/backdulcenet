namespace DulceAtardecer.Models.Dtos.Reportes;

public record TopProductsQueryDto(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Limit = 10
);
