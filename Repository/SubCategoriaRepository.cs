using DulceAtardecer.Common.Exceptions;
using DulceAtardecer.Data;
using DulceAtardecer.Models;
using DulceAtardecer.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DulceAtardecer.Repository;

public class SubCategoriaRepository(ApplicationDbContext db) : ISubCategoriaRepository
{
    public async Task<IEnumerable<SubCategoria>> GetAllAsync(int? categoriaId, CancellationToken cancellationToken = default)
    {
        IQueryable<SubCategoria> query = db.SubCategorias.AsNoTracking().Include(sc => sc.Categoria);

        if (categoriaId.HasValue)
        {
            query = query.Where(sc => sc.CategoriaId == categoriaId.Value);
        }

        return await query.OrderBy(sc => sc.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<SubCategoria> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        SubCategoria? subCategoria = await db.SubCategorias.AsNoTracking().Include(sc => sc.Categoria)
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);
        return subCategoria ?? throw new NotFoundException(nameof(SubCategoria), id);
    }

    public async Task<SubCategoria> CreateAsync(SubCategoria subCategoria, CancellationToken cancellationToken = default)
    {
        db.SubCategorias.Add(subCategoria);
        await db.SaveChangesAsync(cancellationToken);
        return subCategoria;
    }

    public async Task UpdateAsync(int id, SubCategoria subCategoria, CancellationToken cancellationToken = default)
    {
        SubCategoria existing = await db.SubCategorias.FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(SubCategoria), id);

        existing.Nombre = subCategoria.Nombre;
        existing.CategoriaId = subCategoria.CategoriaId;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        SubCategoria existing = await db.SubCategorias.FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(SubCategoria), id);

        db.SubCategorias.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
