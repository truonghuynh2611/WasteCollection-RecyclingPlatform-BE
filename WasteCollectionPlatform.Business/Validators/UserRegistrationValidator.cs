using FluentValidation;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.DTOs.Request.Auth;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Business.Validators;

/// <summary>
/// Validator for user registration requests
/// </summary>
public class UserRegistrationValidator : AbstractValidator<RegisterRequestDto>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.RequiredField, "Full name"))
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.RequiredField, "Email"))
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmailFormat)
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.RequiredField, "Password"))
            .MinimumLength(AppSettings.MinPasswordLength).WithMessage($"Password must be at least {AppSettings.MinPasswordLength} characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage(ErrorMessages.PasswordComplexity);
        
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(string.Format(ErrorMessages.RequiredField, "Phone number"))
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
        
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage(ErrorMessages.InvalidRole);
        
        // Collector-specific validation (PostgreSQL schema uses Team-based structure)
        When(x => x.Role == UserRole.Collector, () =>
        {
            RuleFor(x => x.TeamId)
                .NotEmpty().WithMessage("Team ID is required for Collector registration.");
        });
    }
}
