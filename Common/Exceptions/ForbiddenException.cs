namespace DulceAtardecer.Common.Exceptions;

public class ForbiddenException(string message)
    : AppException("FORBIDDEN", message, StatusCodes.Status403Forbidden);
