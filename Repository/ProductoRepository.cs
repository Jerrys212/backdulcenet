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

    public async Task<Producto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Productos.AsNoTracking().Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Producto> CreateAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        db.Productos.Add(producto);
        await db.SaveChangesAsync(cancellationToken);
        return producto;
    }

    public async Task<bool> UpdateAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        db.Productos.Update(producto);
        return await db.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Producto? producto = await db.Productos.FindAsync([id], cancellationToken);
        if (producto is null)
        {
            return false;
        }

        db.Productos.Remove(producto);
        return await db.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Productos.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> CategoriaExistsAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await db.Categorias.AnyAsync(c => c.Id == categoriaId, cancellationToken);
    }
}
