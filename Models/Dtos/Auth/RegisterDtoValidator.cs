using DulceAtardecer.Repository.IRepository;
using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MustAsync(async (username, ct) => await userRepository.IsUniqueUserAsync(username, ct))
            .WithMessage("El nombre de usuario ya está en uso.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.");
    }
}
