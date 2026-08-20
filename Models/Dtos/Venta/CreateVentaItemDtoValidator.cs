using DulceAtardecer.Repository.IRepository;
using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Venta;

public class CreateVentaItemDtoValidator : AbstractValidator<CreateVentaItemDto>
{
    public CreateVentaItemDtoValidator(IProductoRepository productoRepository, IExtraRepository extraRepository)
    {
        RuleFor(x => x.ProductoId)
            .MustAsync(async (id, ct) => await productoRepository.ExistsAsync(id, ct))
            .WithMessage("El producto indicado no existe.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

        RuleForEach(x => x.ExtraIds)
            .MustAsync(async (id, ct) => await extraRepository.ExistsAsync(id, ct))
            .WithMessage("Uno de los extras indicados no existe.");
    }
}
