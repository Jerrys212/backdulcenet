using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface IProductoRepository
{
    Task<(IEnumerable<Producto> Items, int Total)> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default);
    Task<Producto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Producto> CreateAsync(Producto producto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, Producto producto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>True si el producto existe y está activo (usado para validar items de Venta).</summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
