using DulceAtardecer.Repository.IRepository;
using FluentValidation;

namespace DulceAtardecer.Models.Dtos.SubCategoria;

public class CreateSubCategoriaDtoValidator : AbstractValidator<CreateSubCategoriaDto>
{
    public CreateSubCategoriaDtoValidator(ICategoriaRepository categoriaRepository)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.CategoriaId)
            .MustAsync(async (categoriaId, ct) => await categoriaRepository.ExistsAsync(categoriaId, ct))
            .WithMessage("La categoría indicada no existe.");
    }
}
