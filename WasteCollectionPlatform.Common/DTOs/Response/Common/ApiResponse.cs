namespace WasteCollectionPlatform.Common.DTOs.Response.Common;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public T? Data { get; set; }
    
    public List<string>? Errors { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public static ApiResponse<T> SuccessResponse(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
    
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

/// <summary>
/// Non-generic API response for operations without return data
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public List<string>? Errors { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public static ApiResponse SuccessResponse(string message = "")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }
    
    public static ApiResponse ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
