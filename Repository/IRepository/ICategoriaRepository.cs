using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Categoria> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Categoria> CreateAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, Categoria categoria, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
