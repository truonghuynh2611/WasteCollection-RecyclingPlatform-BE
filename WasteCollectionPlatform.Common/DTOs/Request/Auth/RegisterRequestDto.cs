using System.ComponentModel.DataAnnotations;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Request.Auth;

/// <summary>
/// Registration request data transfer object
/// </summary>
public class RegisterRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number, and one special character.")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must start with 0 and contain exactly 10 digits.")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required.")]
    public UserRole Role { get; set; }
}

