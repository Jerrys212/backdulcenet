using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Reporte;

public class TopProductsRequestDtoValidator : AbstractValidator<TopProductsRequestDto>
{
    public TopProductsRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin.")
            .OverridePropertyName("EndDate");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("El límite debe ser mayor a 0.")
            .LessThanOrEqualTo(100).WithMessage("El límite no puede superar 100.")
            .When(x => x.Limit.HasValue);
    }
}
