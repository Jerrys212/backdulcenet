namespace DulceAtardecer.Common.Exceptions;

public class ConflictException(string message)
    : AppException("CONFLICT", message, StatusCodes.Status409Conflict);
