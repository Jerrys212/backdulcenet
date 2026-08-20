using DulceAtardecer.Common.Time;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Reporte;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class ReporteRepository(ApplicationDbContext db) : IReporteRepository
{
    public async Task<DailyReporteDto> GetDailyAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        (DateTime startUtc, DateTime endUtc) = BusinessTimeZone.GetBusinessDayRange(now);

        List<Venta> sales = await db.Ventas.AsNoTracking()
            .Include(v => v.Items)
            .Where(v => v.FechaCreacion >= startUtc && v.FechaCreacion <= endUtc)
            .ToListAsync(cancellationToken);

        decimal totalAmount = sales.Sum(s => s.Total);
        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats = AggregateProductStats(sales.SelectMany(s => s.Items));
        List<ReportProductDto> topProducts = await ResolveProductStatsAsync(stats, descending: true, limit: 10, cancellationToken);

        return new DailyReporteDto(BusinessTimeZone.ToBusinessDateKey(now), totalAmount, topProducts);
    }

    public async Task<DateRangeReporteDto> GetDateRangeAsync(
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        List<Venta> sales = await FilterByDateRange(db.Ventas.AsNoTracking().Include(v => v.Items), startDate, endDate)
            .ToListAsync(cancellationToken);

        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats = AggregateProductStats(sales.SelectMany(s => s.Items));
        List<ReportProductDto> topProducts = await ResolveProductStatsAsync(stats, descending: true, limit: 10, cancellationToken);
        List<ReportProductDto> leastSoldProducts = await ResolveProductStatsAsync(stats, descending: false, limit: 10, cancellationToken);

        return new DateRangeReporteDto(topProducts, leastSoldProducts);
    }

    public async Task<TopProductsReporteDto> GetTopProductsAsync(
        DateTime? startDate, DateTime? endDate, int limit, CancellationToken cancellationToken = default)
    {
        List<Venta> sales = await FilterByDateRange(db.Ventas.AsNoTracking().Include(v => v.Items), startDate, endDate)
            .ToListAsync(cancellationToken);

        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats = AggregateProductStats(sales.SelectMany(s => s.Items));
        List<ReportProductDto> topProducts = await ResolveProductStatsAsync(stats, descending: true, limit, cancellationToken);

        List<int> soldProductIds = stats.Keys.ToList();
        List<NotSoldProductDto> notSoldProducts = await db.Productos.AsNoTracking()
            .Where(p => p.Activo && !soldProductIds.Contains(p.Id))
            .Select(p => new NotSoldProductDto(p.Id, p.Nombre))
            .ToListAsync(cancellationToken);

        return new TopProductsReporteDto(topProducts, notSoldProducts);
    }

    public async Task<IEnumerable<CategoryPerformanceDto>> GetCategoryPerformanceAsync(
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        List<Venta> sales = await FilterByDateRange(db.Ventas.AsNoTracking().Include(v => v.Items), startDate, endDate)
            .ToListAsync(cancellationToken);

        if (sales.Count == 0)
        {
            return [];
        }

        List<int> productIds = sales.SelectMany(s => s.Items).Select(i => i.ProductoId).Distinct().ToList();
        Dictionary<int, Producto> productos = await db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var acumulado = new Dictionary<int, (string CategoryName, int ItemsSold, decimal Total, HashSet<int> ProductIds, HashSet<int> SaleIds)>();

        foreach (Venta sale in sales)
        {
            foreach (VentaItem item in sale.Items)
            {
                // Si el producto ya no existe o quedó inactivo (equivalente a "borrado" en el
                // modelo original con hard-delete) o no tiene categoría resoluble, se excluye
                // silenciosamente del reporte de categorías, sin fallar la request.
                if (!productos.TryGetValue(item.ProductoId, out Producto? producto)
                    || !producto.Activo
                    || producto.Categoria is null)
                {
                    continue;
                }

                int categoriaId = producto.Categoria.Id;
                if (!acumulado.TryGetValue(categoriaId, out (string CategoryName, int ItemsSold, decimal Total, HashSet<int> ProductIds, HashSet<int> SaleIds) entry))
                {
                    entry = (producto.Categoria.Nombre, 0, 0m, [], []);
                }

                entry.ItemsSold += item.Cantidad;
                entry.Total += item.Subtotal;
                entry.ProductIds.Add(item.ProductoId);
                entry.SaleIds.Add(sale.Id);
                acumulado[categoriaId] = entry;
            }
        }

        decimal grandTotal = sales.Sum(s => s.Total);

        return acumulado.Select(kv =>
        {
            int salesCount = kv.Value.SaleIds.Count;
            decimal averagePerSale = salesCount == 0 ? 0 : Math.Round(kv.Value.Total / salesCount, 2);
            decimal percentOfTotalSales = grandTotal == 0 ? 0 : Math.Round(kv.Value.Total / grandTotal * 100, 2);

            return new CategoryPerformanceDto(
                kv.Key,
                kv.Value.CategoryName,
                kv.Value.ItemsSold,
                kv.Value.Total,
                kv.Value.ProductIds.Count,
                salesCount,
                averagePerSale,
                percentOfTotalSales);
        })
        .OrderByDescending(c => c.Total)
        .ToList();
    }

    public async Task<IEnumerable<UserPerformanceDto>> GetUserPerformanceAsync(
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        List<Venta> sales = await FilterByDateRange(db.Ventas.AsNoTracking(), startDate, endDate)
            .ToListAsync(cancellationToken);

        // El origen (Nest/Mongo) filtraba por usuario activo + permiso "ventas" u "admin".
        // Este proyecto todavía no tiene un flag de usuario activo/inactivo, y cualquier
        // autenticado (Admin o User) ya puede operar como vendedor (ver módulo Ventas) —
        // por eso acá se listan todos los usuarios registrados, sin filtro adicional.
        List<ApplicationUser> usuarios = await db.Users.AsNoTracking().ToListAsync(cancellationToken);

        var resultado = new List<UserPerformanceDto>();
        foreach (ApplicationUser usuario in usuarios)
        {
            List<Venta> theirSales = sales.Where(s => s.SellerId == usuario.Id).ToList();

            var dayTotals = new Dictionary<string, decimal>();
            foreach (Venta sale in theirSales)
            {
                string dayKey = BusinessTimeZone.ToBusinessDateKey(sale.FechaCreacion);
                dayTotals[dayKey] = dayTotals.GetValueOrDefault(dayKey) + sale.Total;
            }

            List<DailySalesEntryDto> dailySales = dayTotals
                .OrderBy(kv => kv.Key)
                .Select(kv => new DailySalesEntryDto(kv.Key, kv.Value))
                .ToList();

            DailySalesEntryDto? bestDay = dailySales.Count == 0
                ? null
                : dailySales.OrderByDescending(d => d.Total).First();

            decimal totalSold = theirSales.Sum(s => s.Total);
            decimal averagePerSale = theirSales.Count == 0 ? 0 : Math.Round(totalSold / theirSales.Count, 2);

            resultado.Add(new UserPerformanceDto(
                usuario.Id, usuario.UserName ?? string.Empty, dailySales, bestDay, dailySales.Count, averagePerSale));
        }

        return resultado;
    }

    private static IQueryable<Venta> FilterByDateRange(IQueryable<Venta> query, DateTime? startDate, DateTime? endDate)
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

    private static Dictionary<int, (int QuantitySold, decimal TotalAmount)> AggregateProductStats(IEnumerable<VentaItem> items)
    {
        var stats = new Dictionary<int, (int QuantitySold, decimal TotalAmount)>();
        foreach (VentaItem item in items)
        {
            stats.TryGetValue(item.ProductoId, out (int QuantitySold, decimal TotalAmount) current);
            stats[item.ProductoId] = (current.QuantitySold + item.Cantidad, current.TotalAmount + item.Subtotal);
        }

        return stats;
    }

    private async Task<List<ReportProductDto>> ResolveProductStatsAsync(
        Dictionary<int, (int QuantitySold, decimal TotalAmount)> stats,
        bool descending,
        int limit,
        CancellationToken cancellationToken)
    {
        if (stats.Count == 0)
        {
            return [];
        }

        List<int> productIds = stats.Keys.ToList();
        Dictionary<int, Producto> productos = await db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        IEnumerable<KeyValuePair<int, (int QuantitySold, decimal TotalAmount)>> ordered = descending
            ? stats.OrderByDescending(kv => kv.Value.QuantitySold)
            : stats.OrderBy(kv => kv.Value.QuantitySold);

        return ordered.Take(limit).Select(kv =>
        {
            productos.TryGetValue(kv.Key, out Producto? producto);
            bool disponible = producto is not null && producto.Activo;

            return new ReportProductDto(
                kv.Key,
                disponible ? producto!.Nombre : "(producto eliminado)",
                disponible ? producto!.Categoria?.Nombre : null,
                kv.Value.QuantitySold,
                kv.Value.TotalAmount);
        }).ToList();
    }
}
