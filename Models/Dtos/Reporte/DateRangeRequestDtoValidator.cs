using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Reporte;

public class DateRangeRequestDtoValidator : AbstractValidator<DateRangeRequestDto>
{
    public DateRangeRequestDtoValidator()
    {
        RuleFor(x => x.StartDate).NotNull().WithMessage("La fecha de inicio es obligatoria.");
        RuleFor(x => x.EndDate).NotNull().WithMessage("La fecha de fin es obligatoria.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin.")
            .OverridePropertyName("EndDate");
    }
}
