using System.Net;
using System.Text.Json;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.Exceptions;

namespace WasteCollectionPlatform.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>();
        
        switch (exception)
        {
            case UnauthorizedException unauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = ApiResponse<object>.ErrorResponse(unauthorizedException.Message);
                _logger.LogWarning(unauthorizedException, "Unauthorized access attempt");
                break;
            
            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ApiResponse<object>.ErrorResponse(notFoundException.Message);
                _logger.LogWarning(notFoundException, "Resource not found");
                break;
            
            case BusinessRuleException businessRuleException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse<object>.ErrorResponse(
                    businessRuleException.Message, 
                    businessRuleException.Errors
                );
                _logger.LogWarning(businessRuleException, "Business rule violation");
                break;
            
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError);
                _logger.LogError(exception, "Unhandled exception occurred");
                
                // Debug: Write exception to file
                try {
                    System.IO.File.WriteAllText(@"d:\SWP391_SP26\crash.txt", exception.ToString());
                } catch {}
                
                break;
        }
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }
}
