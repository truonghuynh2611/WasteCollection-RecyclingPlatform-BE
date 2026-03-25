namespace WasteCollectionPlatform.Common.Constants;

/// <summary>
/// Centralized success messages
/// </summary>
public static class SuccessMessages
{
    // Authentication
    public const string LoginSuccess = "Login successful.";
    public const string RegisterSuccess = "Registration successful.";
    public const string RegisterSuccessPending = "Registration successful. Please wait for admin approval.";
    public const string LogoutSuccess = "Logout successful.";
    public const string TokenRefreshed = "Token refreshed successfully.";
    
    // User management
    public const string UserCreated = "User created successfully.";
    public const string UserUpdated = "User updated successfully.";
    public const string UserDeleted = "User deleted successfully.";
    public const string PasswordChanged = "Password changed successfully.";
    
    // General
    public const string OperationSuccess = "Operation completed successfully.";
}
