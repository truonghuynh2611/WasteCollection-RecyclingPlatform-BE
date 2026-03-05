namespace WasteCollectionPlatform.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : Exception
{
    public List<string> Errors { get; }
    
    public BusinessRuleException() : base("Business rule violation.")
    {
        Errors = new List<string>();
    }
    
    public BusinessRuleException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }
    
    public BusinessRuleException(string message, List<string> errors) : base(message)
    {
        Errors = errors ?? new List<string>();
    }
    
    public BusinessRuleException(string message, Exception innerException) 
        : base(message, innerException)
    {
        Errors = new List<string> { message };
    }
}
