using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Venta;

public class CreateVentaDtoValidator : AbstractValidator<CreateVentaDto>
{
    public CreateVentaDtoValidator(IValidator<CreateVentaItemDto> itemValidator)
    {
        RuleFor(x => x.Cliente)
            .NotEmpty().WithMessage("El cliente es obligatorio.")
            .MaximumLength(100).WithMessage("El cliente no puede superar los 100 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La venta debe tener al menos un item.");

        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
