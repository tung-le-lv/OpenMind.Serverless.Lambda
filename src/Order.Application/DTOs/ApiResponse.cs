namespace Order.Application.DTOs;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string>? Errors { get; init; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}
