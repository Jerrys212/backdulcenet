using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class ProductoRepository(ApplicationDbContext db) : IProductoRepository
{
    public async Task<IEnumerable<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Productos.AsNoTracking().Include(p => p.Categoria).ToListAsync(cancellationToken);
    }

    public async Task<Producto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Producto? producto = await db.Productos.AsNoTracking().Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return producto ?? throw new NotFoundException(nameof(Producto), id);
    }

    public async Task<Producto> CreateAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        db.Productos.Add(producto);
        await db.SaveChangesAsync(cancellationToken);
        return producto;
    }

    public async Task UpdateAsync(int id, Producto producto, CancellationToken cancellationToken = default)
    {
        Producto existing = await db.Productos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Producto), id);

        existing.Nombre = producto.Nombre;
        existing.Descripcion = producto.Descripcion;
        existing.Precio = producto.Precio;
        existing.CategoriaId = producto.CategoriaId;
        if (!string.IsNullOrEmpty(producto.ImgUrl))
        {
            existing.ImgUrl = producto.ImgUrl;
            existing.ImgUrlLocal = producto.ImgUrlLocal;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Producto existing = await db.Productos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Producto), id);

        db.Productos.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
