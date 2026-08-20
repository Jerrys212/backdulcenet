using DulceAtardecer.Constants;
using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Venta;

public class UpdateVentaEstadoDtoValidator : AbstractValidator<UpdateVentaEstadoDto>
{
    public UpdateVentaEstadoDtoValidator()
    {
        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es obligatorio.")
            .Must(VentaEstados.EsValido)
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", VentaEstados.GetAll())}.");
    }
}
