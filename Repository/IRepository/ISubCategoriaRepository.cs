using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface ISubCategoriaRepository
{
    Task<IEnumerable<SubCategoria>> GetAllAsync(int? categoriaId, CancellationToken cancellationToken = default);
    Task<SubCategoria> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SubCategoria> CreateAsync(SubCategoria subCategoria, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, SubCategoria subCategoria, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>True si la subcategoría existe y pertenece a la categoría indicada (usado para validar Producto).</summary>
    Task<bool> ExistsAsync(int id, int categoriaId, CancellationToken cancellationToken = default);
}
