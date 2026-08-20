using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface IExtraRepository
{
    Task<(IEnumerable<Extra> Items, int Total)> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default);
    Task<Extra> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Extra> CreateAsync(Extra extra, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, Extra extra, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>True si el extra existe y está activo (para validar referencias desde Ventas).</summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
