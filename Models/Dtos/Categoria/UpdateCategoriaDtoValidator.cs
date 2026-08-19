using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Categoria;

public class UpdateCategoriaDtoValidator : AbstractValidator<UpdateCategoriaDto>
{
    public UpdateCategoriaDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
    }
}
