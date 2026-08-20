using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Reportes;

public class TopProductsQueryDtoValidator : AbstractValidator<TopProductsQueryDto>
{
    public TopProductsQueryDtoValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("El límite debe ser un entero entre 1 y 100.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithName("startDate")
            .WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin.");
    }
}
