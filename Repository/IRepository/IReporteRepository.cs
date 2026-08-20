using DulceAtardecer.Models.Dtos.Reporte;

namespace DulceAtardecer.Repository.IRepository;

/// <summary>
/// Reportes es de solo lectura: no posee entidad propia, solo consulta Ventas/Productos/Usuarios.
/// Ver docs/reportes.md para el detalle de cada cálculo (portado desde el módulo NestJS original).
/// </summary>
public interface IReporteRepository
{
    Task<DailyReporteDto> GetDailyAsync(CancellationToken cancellationToken = default);

    Task<DateRangeReporteDto> GetDateRangeAsync(
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<TopProductsReporteDto> GetTopProductsAsync(
        DateTime? startDate, DateTime? endDate, int limit, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryPerformanceDto>> GetCategoryPerformanceAsync(
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<UserPerformanceDto>> GetUserPerformanceAsync(
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}
