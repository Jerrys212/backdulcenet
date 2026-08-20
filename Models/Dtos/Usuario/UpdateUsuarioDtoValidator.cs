using DulceAtardecer.Constants;
using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Usuario;

public class UpdateUsuarioDtoValidator : AbstractValidator<UpdateUsuarioDto>
{
    public UpdateUsuarioDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(Roles.EsValido).WithMessage($"El rol debe ser uno de: {string.Join(", ", Roles.GetAll())}.");
    }
}
