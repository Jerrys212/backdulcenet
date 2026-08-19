namespace DulceAtardecer.Common.Exceptions;

public class NotFoundException(string entity, object key)
    : AppException("NOT_FOUND", $"{entity} con id '{key}' no encontrado.", StatusCodes.Status404NotFound);
