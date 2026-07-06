using FluentValidation;

namespace TaskManagement.Api.Application.Tasks;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(c => c.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");
        RuleFor(c => c.Description)
            .Must(desc => !string.IsNullOrWhiteSpace(desc)).WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.");
        RuleFor(c => c.Priority).IsInEnum().WithMessage("Priority is invalid.");
    }
}

public class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(c => c.CompletedDescription)
            .MaximumLength(2000).WithMessage("Completed description must be 2000 characters or fewer.");
    }
}
