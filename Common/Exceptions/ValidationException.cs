using FluentValidation.Results;

namespace DulceAtardecer.Common.Exceptions;

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("VALIDATION_ERROR", "Uno o más campos no son válidos.", StatusCodes.Status400BadRequest)
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "Uno o más campos no son válidos.", StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }
}
