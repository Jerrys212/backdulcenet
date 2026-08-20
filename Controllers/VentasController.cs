using System.Security.Claims;
using Asp.Versioning;
using DulceAtardecer.Common.Responses;
using DulceAtardecer.Constants;
using DulceAtardecer.Models;
using DulceAtardecer.Models.Dtos.Venta;
using DulceAtardecer.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DulceAtardecer.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Authorize]
public class VentasController(
    IVentaRepository ventaRepository,
    IProductoRepository productoRepository,
    IExtraRepository extraRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VentaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VentaDto>>>> GetAllAsync(
        [FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        (IEnumerable<Venta> items, int total) = await ventaRepository.GetAllAsync(page, limit, cancellationToken);
        IEnumerable<VentaDto> dtos = items.Adapt<IEnumerable<VentaDto>>();
        return Ok(new ApiResponse<IEnumerable<VentaDto>>(true, dtos, new ApiMeta(page, total)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VentaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VentaDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Venta venta = await ventaRepository.GetByIdAsync(id, cancellationToken);
        return Ok(venta.Adapt<VentaDto>());
    }

    [HttpPost]
    [ProducesResponseType(typeof(VentaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VentaDto>> CreateAsync(CreateVentaDto createDto, CancellationToken cancellationToken)
    {
        string sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");

        DateTime now = DateTime.UtcNow;
        var venta = new Venta
        {
            Cliente = createDto.Cliente,
            SellerId = sellerId,
            Estado = VentaEstados.Pendiente,
            EstadoActualizadoEn = now,
            EstadoActualizadoPorId = sellerId,
            FechaCreacion = now,
            FechaActualizacion = now
        };

        foreach (CreateVentaItemDto itemDto in createDto.Items)
        {
            Producto producto = await productoRepository.GetByIdAsync(itemDto.ProductoId, cancellationToken);

            var ventaItem = new VentaItem
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Cantidad = itemDto.Cantidad
            };

            foreach (int extraId in itemDto.ExtraIds)
            {
                Extra extra = await extraRepository.GetByIdAsync(extraId, cancellationToken);
                ventaItem.Extras.Add(new VentaItemExtra { ExtraId = extra.Id, Precio = extra.Precio });
            }

            venta.Items.Add(ventaItem);
        }

        // Subtotal/Total los recalcula y garantiza VentaRepository.CreateAsync.
        venta = await ventaRepository.CreateAsync(venta, cancellationToken);
        Venta ventaCreada = await ventaRepository.GetByIdAsync(venta.Id, cancellationToken);
        VentaDto result = ventaCreada.Adapt<VentaDto>();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEstadoAsync(int id, UpdateVentaEstadoDto updateDto, CancellationToken cancellationToken)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");

        await ventaRepository.UpdateEstadoAsync(id, updateDto.Estado, userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await ventaRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
