using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface ICategoriaRepository
{
    Task<(IEnumerable<Categoria> Items, int Total)> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default);
    Task<Categoria> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Categoria> CreateAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, Categoria categoria, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>True si la categoría existe y está activa (usado para validar FKs de Producto/SubCategoria).</summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
