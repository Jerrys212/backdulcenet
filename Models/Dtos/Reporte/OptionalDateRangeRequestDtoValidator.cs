using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Reporte;

public class OptionalDateRangeRequestDtoValidator : AbstractValidator<OptionalDateRangeRequestDto>
{
    public OptionalDateRangeRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin.")
            .OverridePropertyName("EndDate");
    }
}
