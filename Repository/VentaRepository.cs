using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class VentaRepository(ApplicationDbContext db) : IVentaRepository
{
    public async Task<(IEnumerable<Venta> Items, int Total)> GetAllAsync(
        int page, int limit, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        IQueryable<Venta> query = db.Ventas.AsNoTracking()
            .Include(v => v.Seller)
            .Include(v => v.EstadoActualizadoPor)
            .Include(v => v.Items).ThenInclude(i => i.Extras).ThenInclude(e => e.Extra)
            .OrderByDescending(v => v.FechaCreacion);

        int total = await query.CountAsync(cancellationToken);
        List<Venta> items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Venta> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Venta? venta = await db.Ventas.AsNoTracking()
            .Include(v => v.Seller)
            .Include(v => v.EstadoActualizadoPor)
            .Include(v => v.Items).ThenInclude(i => i.Extras).ThenInclude(e => e.Extra)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        return venta ?? throw new NotFoundException(nameof(Venta), id);
    }

    public async Task<Venta> CreateAsync(Venta venta, CancellationToken cancellationToken = default)
    {
        // Recalcula Subtotal/Total acá para que el invariante "Total = suma de subtotales,
        // Subtotal = precio*cantidad + extras" quede garantizado por el repositorio, sin
        // depender de que quien llame (hoy solo VentasController) haya hecho bien la cuenta.
        foreach (VentaItem item in venta.Items)
        {
            item.Subtotal = (item.Precio * item.Cantidad) + item.Extras.Sum(e => e.Precio);
        }

        venta.Total = venta.Items.Sum(i => i.Subtotal);

        db.Ventas.Add(venta);
        await db.SaveChangesAsync(cancellationToken);
        return venta;
    }

    public async Task UpdateEstadoAsync(
        int id, string estado, string estadoActualizadoPorId, CancellationToken cancellationToken = default)
    {
        Venta existing = await db.Ventas.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Venta), id);

        DateTime now = DateTime.UtcNow;
        existing.Estado = estado;
        existing.EstadoActualizadoEn = now;
        existing.EstadoActualizadoPorId = estadoActualizadoPorId;
        existing.FechaActualizacion = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Venta existing = await db.Ventas.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Venta), id);

        db.Ventas.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
