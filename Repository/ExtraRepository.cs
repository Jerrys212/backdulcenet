using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class ExtraRepository(ApplicationDbContext db) : IExtraRepository
{
    public async Task<(IEnumerable<Extra> Items, int Total)> GetAllAsync(
        int page, int limit, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        IQueryable<Extra> query = db.Extras.AsNoTracking().Where(e => e.Activo).OrderBy(e => e.Nombre);

        int total = await query.CountAsync(cancellationToken);
        List<Extra> items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Extra> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Extra? extra = await db.Extras.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && e.Activo, cancellationToken);
        return extra ?? throw new NotFoundException(nameof(Extra), id);
    }

    public async Task<Extra> CreateAsync(Extra extra, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        extra.Activo = true;
        extra.FechaCreacion = now;
        extra.FechaActualizacion = now;

        db.Extras.Add(extra);
        await db.SaveChangesAsync(cancellationToken);
        return extra;
    }

    public async Task UpdateAsync(int id, Extra extra, CancellationToken cancellationToken = default)
    {
        Extra existing = await db.Extras.FirstOrDefaultAsync(e => e.Id == id && e.Activo, cancellationToken)
            ?? throw new NotFoundException(nameof(Extra), id);

        existing.Nombre = extra.Nombre;
        existing.Precio = extra.Precio;
        existing.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Extra existing = await db.Extras.FirstOrDefaultAsync(e => e.Id == id && e.Activo, cancellationToken)
            ?? throw new NotFoundException(nameof(Extra), id);

        existing.Activo = false;
        existing.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Extras.AnyAsync(e => e.Id == id && e.Activo, cancellationToken);
    }
}
