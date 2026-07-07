using FluentValidation;

namespace TaskManagement.Api.Application.Auth;

// Length rules validate the trimmed value: the handler trims before saving, so
// leading/trailing whitespace can't be used to satisfy a rule.
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public const int EmailMaxLength = 256;
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int PasswordMinLength = 6;

    // text@text.text with no spaces — mirrors EMAIL_REGEX on the frontend so the
    // two layers accept exactly the same addresses.
    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(EmailPattern).WithMessage("Email must be a valid email address.")
            .Must(e => e.Trim().Length <= EmailMaxLength).WithMessage($"Email must be {EmailMaxLength} characters or fewer.");

        RuleFor(c => c.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(PasswordMinLength).WithMessage($"Password must be at least {PasswordMinLength} characters.");

        RuleFor(c => c.FirstName)
            .Cascade(CascadeMode.Stop)
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("First name is required.")
            .Must(n => n.Trim().Length >= NameMinLength).WithMessage($"First name must be at least {NameMinLength} characters.")
            .Must(n => n.Trim().Length <= NameMaxLength).WithMessage($"First name must be {NameMaxLength} characters or fewer.");

        RuleFor(c => c.LastName)
            .Cascade(CascadeMode.Stop)
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Last name is required.")
            .Must(n => n.Trim().Length >= NameMinLength).WithMessage($"Last name must be at least {NameMinLength} characters.")
            .Must(n => n.Trim().Length <= NameMaxLength).WithMessage($"Last name must be {NameMaxLength} characters or fewer.");
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(c => c.Password).NotEmpty().WithMessage("Password is required.");
    }
}
