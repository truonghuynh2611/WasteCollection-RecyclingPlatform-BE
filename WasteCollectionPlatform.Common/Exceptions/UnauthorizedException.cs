namespace WasteCollectionPlatform.Common.Exceptions;

/// <summary>
/// Exception thrown when authentication or authorization fails
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Unauthorized access.")
    {
    }
    
    public UnauthorizedException(string message) : base(message)
    {
    }
    
    public UnauthorizedException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
