using DulceAtardecer.Common.Utils;
using DulceAtardecer.Constants;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Reportes;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Services;

public class ReportesService(ApplicationDbContext db) : IReportesService
{
    public async Task<DailyReportResponseDto> GetDailyAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        (DateTime startUtc, DateTime endUtc) = BusinessTimeZone.GetBusinessDayRange(now);

        List<Venta> ventas = await VentasQuery()
            .Where(v => v.FechaCreacion >= startUtc && v.FechaCreacion <= endUtc)
            .ToListAsync(cancellationToken);

        decimal totalAmount = ventas.Sum(v => v.Total);
        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats = AggregateProductStats(ventas);
        IReadOnlyList<ReportProductDto> topProducts = await ResolveProductStatsAsync(stats, descending: true, limit: 10, cancellationToken);

        return new DailyReportResponseDto(BusinessTimeZone.ToBusinessDateKey(now), totalAmount, topProducts);
    }

    public async Task<DateRangeReportResponseDto> GetDateRangeAsync(DateRangeDto dto, CancellationToken cancellationToken = default)
    {
        List<Venta> ventas = await VentasQuery()
            .Where(v => v.FechaCreacion >= dto.StartDate && v.FechaCreacion <= dto.EndDate)
            .ToListAsync(cancellationToken);

        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats = AggregateProductStats(ventas);
        IReadOnlyList<ReportProductDto> topProducts = await ResolveProductStatsAsync(stats, descending: true, limit: 10, cancellationToken);
        IReadOnlyList<ReportProductDto> leastSoldProducts = await ResolveProductStatsAsync(stats, descending: false, limit: 10, cancellationToken);

        return new DateRangeReportResponseDto(topProducts, leastSoldProducts);
    }

    public async Task<TopProductsReportResponseDto> GetTopProductsAsync(TopProductsQueryDto dto, CancellationToken cancellationToken = default)
    {
        List<Venta> ventas = await FilterByOptionalRange(VentasQuery(), dto.StartDate, dto.EndDate)
            .ToListAsync(cancellationToken);

        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats = AggregateProductStats(ventas);
        IReadOnlyList<ReportProductDto> topProducts = await ResolveProductStatsAsync(stats, descending: true, limit: dto.Limit, cancellationToken);

        List<Producto> activeProducts = await db.Productos.AsNoTracking()
            .Where(p => p.Activo)
            .ToListAsync(cancellationToken);

        List<NotSoldProductDto> notSoldProducts = activeProducts
            .Where(p => !stats.ContainsKey(p.Id))
            .Select(p => new NotSoldProductDto(p.Id.ToString(), p.Nombre))
            .ToList();

        return new TopProductsReportResponseDto(topProducts, notSoldProducts);
    }

    public async Task<IReadOnlyList<CategoryPerformanceItemDto>> GetCategoryPerformanceAsync(
        OptionalDateRangeDto dto, CancellationToken cancellationToken = default)
    {
        List<Venta> ventas = await FilterByOptionalRange(VentasQuery(), dto.StartDate, dto.EndDate)
            .ToListAsync(cancellationToken);

        if (ventas.Count == 0)
        {
            return [];
        }

        HashSet<int> productIds = ventas.SelectMany(v => v.Items).Select(i => i.ProductoId).ToHashSet();
        Dictionary<int, Producto> productos = await db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var acumulado = new Dictionary<int, (string Nombre, int ItemsSold, decimal Total, HashSet<int> ProductIds, HashSet<int> SaleIds)>();

        foreach (Venta venta in ventas)
        {
            foreach (VentaItem item in venta.Items)
            {
                if (!productos.TryGetValue(item.ProductoId, out Producto? producto))
                {
                    continue;
                }

                int categoriaId = producto.CategoriaId;
                if (!acumulado.TryGetValue(categoriaId, out var entry))
                {
                    entry = (producto.Categoria?.Nombre ?? string.Empty, 0, 0m, [], []);
                }

                entry.ItemsSold += item.Cantidad;
                entry.Total += item.Subtotal;
                entry.ProductIds.Add(producto.Id);
                entry.SaleIds.Add(venta.Id);
                acumulado[categoriaId] = entry;
            }
        }

        decimal grandTotal = ventas.Sum(v => v.Total);

        return acumulado
            .Select(kv =>
            {
                (int categoriaId, var entry) = (kv.Key, kv.Value);
                int salesCount = entry.SaleIds.Count;
                decimal averagePerSale = salesCount == 0 ? 0 : entry.Total / salesCount;
                decimal percentOfTotalSales = grandTotal == 0 ? 0 : entry.Total / grandTotal * 100;

                return new CategoryPerformanceItemDto(
                    categoriaId.ToString(),
                    entry.Nombre,
                    entry.ItemsSold,
                    entry.Total,
                    entry.ProductIds.Count,
                    salesCount,
                    averagePerSale,
                    percentOfTotalSales);
            })
            .OrderByDescending(c => c.Total)
            .ToList();
    }

    public async Task<IReadOnlyList<UserPerformanceItemDto>> GetUserPerformanceAsync(
        OptionalDateRangeDto dto, CancellationToken cancellationToken = default)
    {
        List<Venta> ventas = await FilterByOptionalRange(db.Ventas.AsNoTracking(), dto.StartDate, dto.EndDate)
            .ToListAsync(cancellationToken);

        List<string> vendedorRoleIds = await db.Roles
            .Where(r => r.Name == Roles.Admin || r.Name == Roles.User)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        List<string> vendedorUserIds = await db.UserRoles
            .Where(ur => vendedorRoleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<ApplicationUser> usuarios = await db.Users.AsNoTracking()
            .Where(u => vendedorUserIds.Contains(u.Id) && (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow))
            .ToListAsync(cancellationToken);

        var resultado = new List<UserPerformanceItemDto>(usuarios.Count);
        foreach (ApplicationUser usuario in usuarios)
        {
            List<Venta> theirSales = ventas.Where(v => v.SellerId == usuario.Id).ToList();

            List<DailySalesEntryDto> dailySales = theirSales
                .GroupBy(v => BusinessTimeZone.ToBusinessDateKey(v.FechaCreacion))
                .Select(g => new DailySalesEntryDto(g.Key, g.Sum(v => v.Total)))
                .OrderBy(d => d.Date, StringComparer.Ordinal)
                .ToList();

            DailySalesEntryDto? bestDay = dailySales.Count == 0
                ? null
                : dailySales.OrderByDescending(d => d.Total).First();

            decimal totalSold = theirSales.Sum(v => v.Total);
            decimal averagePerSale = theirSales.Count == 0 ? 0 : totalSold / theirSales.Count;

            resultado.Add(new UserPerformanceItemDto(
                usuario.Id,
                usuario.UserName ?? string.Empty,
                dailySales,
                bestDay,
                dailySales.Count,
                averagePerSale));
        }

        return resultado;
    }

    private IQueryable<Venta> VentasQuery() => db.Ventas.AsNoTracking().Include(v => v.Items);

    private static IQueryable<Venta> FilterByOptionalRange(IQueryable<Venta> query, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue)
        {
            query = query.Where(v => v.FechaCreacion >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(v => v.FechaCreacion <= endDate.Value);
        }

        return query;
    }

    private static Dictionary<int, (int QuantitySold, decimal TotalAmount)> AggregateProductStats(IEnumerable<Venta> ventas)
    {
        var stats = new Dictionary<int, (int QuantitySold, decimal TotalAmount)>();

        foreach (Venta venta in ventas)
        {
            foreach (VentaItem item in venta.Items)
            {
                stats.TryGetValue(item.ProductoId, out var current);
                stats[item.ProductoId] = (current.QuantitySold + item.Cantidad, current.TotalAmount + item.Subtotal);
            }
        }

        return stats;
    }

    private async Task<IReadOnlyList<ReportProductDto>> ResolveProductStatsAsync(
        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats,
        bool descending,
        int limit,
        CancellationToken cancellationToken)
    {
        if (stats.Count == 0)
        {
            return [];
        }

        Dictionary<int, Producto> productos = await db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => stats.Keys.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        IEnumerable<KeyValuePair<int, (int QuantitySold, decimal TotalAmount)>> ordered = descending
            ? stats.OrderByDescending(kv => kv.Value.QuantitySold)
            : stats.OrderBy(kv => kv.Value.QuantitySold);

        return ordered
            .Take(limit)
            .Select(kv =>
            {
                if (productos.TryGetValue(kv.Key, out Producto? producto))
                {
                    return new ReportProductDto(
                        producto.Id.ToString(), producto.Nombre, producto.Categoria?.Nombre,
                        kv.Value.QuantitySold, kv.Value.TotalAmount);
                }

                return new ReportProductDto(
                    kv.Key.ToString(), "(producto eliminado)", null, kv.Value.QuantitySold, kv.Value.TotalAmount);
            })
            .ToList();
    }
}
