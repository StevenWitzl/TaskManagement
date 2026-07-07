using FluentValidation;

namespace TaskManagement.Api.Application.Tasks;

// Length rules validate the trimmed value: handlers trim before saving, so
// leading/trailing whitespace can't be used to satisfy a minimum length.
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public const int TitleMinLength = 5;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public CreateTaskCommandValidator()
    {
        RuleFor(c => c.Title)
            .Cascade(CascadeMode.Stop)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Title is required.")
            .Must(t => t.Trim().Length >= TitleMinLength).WithMessage($"Title must be at least {TitleMinLength} characters.")
            .Must(t => t.Trim().Length <= TitleMaxLength).WithMessage($"Title must be {TitleMaxLength} characters or fewer.");

        RuleFor(c => c.Description)
            .Cascade(CascadeMode.Stop)
            .Must(d => !string.IsNullOrWhiteSpace(d)).WithMessage("Description is required.")
            .Must(d => d.Trim().Length <= DescriptionMaxLength).WithMessage($"Description must be {DescriptionMaxLength} characters or fewer.");

        RuleFor(c => c.Priority).IsInEnum().WithMessage("Priority is invalid.");
    }
}

public class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public const int DescriptionMaxLength = 2000;

    public CompleteTaskCommandValidator()
    {
        RuleFor(c => c.CompletedDescription)
            .Must(d => d is null || d.Trim().Length <= DescriptionMaxLength)
            .WithMessage($"Completed description must be {DescriptionMaxLength} characters or fewer.");
    }
}
