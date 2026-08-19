namespace DulceAtardecer.Common.Responses;

public record ApiErrorResponse(bool Success, ApiError Error);

public record ApiError(string Code, string Message, IDictionary<string, string[]>? Details = null);
