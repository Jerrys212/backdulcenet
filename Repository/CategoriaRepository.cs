using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class CategoriaRepository(ApplicationDbContext db) : ICategoriaRepository
{
    public async Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Categorias.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Categoria?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Categoria> CreateAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(cancellationToken);
        return categoria;
    }

    public async Task<bool> UpdateAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        db.Categorias.Update(categoria);
        return await db.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Categoria? categoria = await db.Categorias.FindAsync([id], cancellationToken);
        if (categoria is null)
        {
            return false;
        }

        db.Categorias.Remove(categoria);
        return await db.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Categorias.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
