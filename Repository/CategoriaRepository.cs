using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class CategoriaRepository(ApplicationDbContext db) : ICategoriaRepository
{
    public async Task<(IEnumerable<Categoria> Items, int Total)> GetAllAsync(
        int page, int limit, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        IQueryable<Categoria> query = db.Categorias.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.Nombre);

        int total = await query.CountAsync(cancellationToken);
        List<Categoria> items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Categoria> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Categoria? categoria = await db.Categorias.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo, cancellationToken);
        return categoria ?? throw new NotFoundException(nameof(Categoria), id);
    }

    public async Task<Categoria> CreateAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        categoria.Activo = true;
        categoria.FechaCreacion = now;
        categoria.FechaActualizacion = now;

        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(cancellationToken);
        return categoria;
    }

    public async Task UpdateAsync(int id, Categoria categoria, CancellationToken cancellationToken = default)
    {
        Categoria existing = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.Activo, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), id);

        existing.Nombre = categoria.Nombre;
        existing.Descripcion = categoria.Descripcion;
        existing.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Categoria existing = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.Activo, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), id);

        existing.Activo = false;
        existing.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Categorias.AnyAsync(c => c.Id == id && c.Activo, cancellationToken);
    }
}
