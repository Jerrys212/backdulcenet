using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Producto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Producto> CreateAsync(Producto producto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, Producto producto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
