using DulceAtardecer.Models.Dtos.Reportes;

namespace DulceAtardecer.Services;

/// <summary>
/// Módulo de solo lectura (docs/reportes.md §10): no posee entidades propias, solo consulta
/// Ventas/Productos/Usuarios. Por eso vive en Services/ en vez de Repository/ — no es un CRUD.
/// </summary>
public interface IReportesService
{
    Task<DailyReportResponseDto> GetDailyAsync(CancellationToken cancellationToken = default);
    Task<DateRangeReportResponseDto> GetDateRangeAsync(DateRangeDto dto, CancellationToken cancellationToken = default);
    Task<TopProductsReportResponseDto> GetTopProductsAsync(TopProductsQueryDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryPerformanceItemDto>> GetCategoryPerformanceAsync(OptionalDateRangeDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPerformanceItemDto>> GetUserPerformanceAsync(OptionalDateRangeDto dto, CancellationToken cancellationToken = default);
}
