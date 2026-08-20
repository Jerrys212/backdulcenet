using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Reportes;

public class OptionalDateRangeDtoValidator : AbstractValidator<OptionalDateRangeDto>
{
    public OptionalDateRangeDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithName("startDate")
            .WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin.");
    }
}
