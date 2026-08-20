namespace DulceAtardecer.Common.Exceptions;

public class BusinessRuleException(string code, string message)
    : AppException(code, message, StatusCodes.Status422UnprocessableEntity);
