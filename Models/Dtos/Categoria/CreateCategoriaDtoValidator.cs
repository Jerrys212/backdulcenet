using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Categoria;

public class CreateCategoriaDtoValidator : AbstractValidator<CreateCategoriaDto>
{
    public CreateCategoriaDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
    }
}
