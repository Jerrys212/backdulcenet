using FluentValidation;

namespace DulceAtardecer.Models.Dtos.Reportes;

public class DateRangeDtoValidator : AbstractValidator<DateRangeDto>
{
    public DateRangeDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .WithName("startDate")
            .WithMessage("La fecha de inicio debe ser anterior o igual a la fecha de fin.");
    }
}
