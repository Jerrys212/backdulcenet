using DulceAtardecer.Common.Exceptions;
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

    public async Task<Categoria> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Categoria? categoria = await db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return categoria ?? throw new NotFoundException(nameof(Categoria), id);
    }

    public async Task<Categoria> CreateAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(cancellationToken);
        return categoria;
    }

    public async Task UpdateAsync(int id, Categoria categoria, CancellationToken cancellationToken = default)
    {
        Categoria existing = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), id);

        existing.Nombre = categoria.Nombre;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Categoria existing = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), id);

        db.Categorias.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Categorias.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
