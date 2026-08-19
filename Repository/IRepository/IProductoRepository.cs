using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Producto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Producto> CreateAsync(Producto producto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Producto producto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CategoriaExistsAsync(int categoriaId, CancellationToken cancellationToken = default);
}
