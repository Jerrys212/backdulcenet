using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Extra;

public class CreateExtraDtoValidator : AbstractValidator<CreateExtraDto>
{
    public CreateExtraDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");
    }
}
