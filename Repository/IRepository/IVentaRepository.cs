using DulceAtardecer.Models;

namespace DulceAtardecer.Repository.IRepository;

public interface IVentaRepository
{
    Task<(IEnumerable<Venta> Items, int Total)> GetAllAsync(int page, int limit, CancellationToken cancellationToken = default);
    Task<Venta> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Venta> CreateAsync(Venta venta, CancellationToken cancellationToken = default);
    Task UpdateEstadoAsync(int id, string estado, string estadoActualizadoPorId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
