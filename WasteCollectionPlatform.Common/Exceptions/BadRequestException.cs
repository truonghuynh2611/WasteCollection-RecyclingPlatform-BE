namespace WasteCollectionPlatform.Common.Exceptions;

/// <summary>
/// Exception thrown when request data is invalid
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException() : base("Bad request.")
    {
    }

    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}