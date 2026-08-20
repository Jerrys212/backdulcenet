using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class ProductoRepository(ApplicationDbContext db) : IProductoRepository
{
    public async Task<(IEnumerable<Producto> Items, int Total)> GetAllAsync(
        int page, int limit, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        IQueryable<Producto> query = db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.SubCategoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre);

        int total = await query.CountAsync(cancellationToken);
        List<Producto> items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Producto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Producto? producto = await db.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.SubCategoria)
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo, cancellationToken);
        return producto ?? throw new NotFoundException(nameof(Producto), id);
    }

    public async Task<Producto> CreateAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        producto.Activo = true;
        producto.FechaCreacion = now;
        producto.FechaActualizacion = now;

        db.Productos.Add(producto);
        await db.SaveChangesAsync(cancellationToken);
        return producto;
    }

    public async Task UpdateAsync(int id, Producto producto, CancellationToken cancellationToken = default)
    {
        Producto existing = await db.Productos.FirstOrDefaultAsync(p => p.Id == id && p.Activo, cancellationToken)
            ?? throw new NotFoundException(nameof(Producto), id);

        existing.Nombre = producto.Nombre;
        existing.Descripcion = producto.Descripcion;
        existing.Precio = producto.Precio;
        existing.CategoriaId = producto.CategoriaId;
        existing.SubCategoriaId = producto.SubCategoriaId;
        if (!string.IsNullOrEmpty(producto.ImgUrl))
        {
            existing.ImgUrl = producto.ImgUrl;
            existing.ImgUrlLocal = producto.ImgUrlLocal;
        }

        existing.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Producto existing = await db.Productos.FirstOrDefaultAsync(p => p.Id == id && p.Activo, cancellationToken)
            ?? throw new NotFoundException(nameof(Producto), id);

        existing.Activo = false;
        existing.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
