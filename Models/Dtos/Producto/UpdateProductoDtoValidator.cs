using DulceAtardecer.Repository.IRepository;
using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Producto;

public class UpdateProductoDtoValidator : AbstractValidator<UpdateProductoDto>
{
    public UpdateProductoDtoValidator(ICategoriaRepository categoriaRepository, ISubCategoriaRepository subCategoriaRepository)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(255).WithMessage("La descripción no puede superar los 255 caracteres.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        RuleFor(x => x.CategoriaId)
            .MustAsync(async (categoriaId, ct) => await categoriaRepository.ExistsAsync(categoriaId, ct))
            .WithMessage("La categoría indicada no existe.");

        RuleFor(x => x.SubCategoriaId)
            .MustAsync(async (dto, subCategoriaId, ct) => await subCategoriaRepository.ExistsAsync(subCategoriaId, dto.CategoriaId, ct))
            .WithMessage("La subcategoría indicada no existe o no pertenece a la categoría seleccionada.");
    }
}
