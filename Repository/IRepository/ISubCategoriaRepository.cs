using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface ISubCategoriaRepository
{
    Task<IEnumerable<SubCategoria>> GetAllAsync(int? categoriaId, CancellationToken cancellationToken = default);
    Task<SubCategoria> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SubCategoria> CreateAsync(SubCategoria subCategoria, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, SubCategoria subCategoria, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
