namespace WasteCollectionPlatform.Common.Constants;

/// <summary>
/// Centralized error messages
/// </summary>
public static class ErrorMessages
{
    // Authentication errors
    public const string InvalidCredentials = "Invalid email or password.";
    public const string EmailAlreadyExists = "Email already exists.";
    public const string UserNotFound = "User not found.";
    public const string InvalidToken = "Invalid or expired token.";
    public const string UnauthorizedAccess = "You are not authorized to perform this action.";
    public const string AccountInactive = "Your account is inactive. Please contact support.";
    public const string AccountPending = "Your account is pending approval. Please wait for admin approval.";
    public const string AccountSuspended = "Your account has been suspended. Please contact support.";
    
    // Validation errors
    public const string RequiredField = "{0} is required.";
    public const string InvalidEmailFormat = "Invalid email format.";
    public const string InvalidPhoneFormat = "Invalid phone number format.";
    public const string PasswordTooShort = "Password must be at least 8 characters long.";
    public const string PasswordComplexity = "Password must contain at least one uppercase, one lowercase, one number, and one special character.";
    public const string InvalidRole = "Invalid user role specified.";
    
    // Business rule errors
    public const string DistrictRequired = "District is required for Enterprise and Collector roles.";
    public const string ServiceAreaRequired = "Service area is required for Enterprise role.";
    public const string WasteTypesRequired = "Waste types accepted is required for Enterprise role.";
    public const string CapacityLimitExceeded = "Enterprise has reached its daily capacity limit.";
    
    // General errors
    public const string InternalServerError = "An unexpected error occurred. Please try again later.";
    public const string DatabaseError = "A database error occurred. Please try again later.";
}
