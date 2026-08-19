namespace DulceAtardecer.Common.Responses;

public record ApiResponse<T>(bool Success, T? Data, ApiMeta? Meta = null);

public record ApiMeta(int Page, int Total);
