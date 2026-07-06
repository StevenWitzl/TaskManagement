using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Application.Tasks;

public record TaskDto(
    Guid Id,
    int Order,
    Priority Priority,
    string Title,
    string Description,
    DateTime CreatedDate,
    DateTime? CompletedDate,
    string? CompletedDescription);

public record CreateTaskRequestDto(string Title, string Description, Priority Priority);

public record CompleteTaskRequestDto(string? CompletedDescription);

public record ReorderTaskRequestDto(int NewOrder);
