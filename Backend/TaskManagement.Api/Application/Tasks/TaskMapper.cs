using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Application.Tasks;

public static class TaskMapper
{
    public static TaskDto ToDto(this TaskItem task) => new(
        task.Id,
        task.Order,
        task.Priority,
        task.Title,
        task.Description,
        task.CreatedDate,
        task.CompletedDate,
        task.CompletedDescription);

    public static List<TaskDto> ToDtos(this IEnumerable<TaskItem> tasks) =>
        tasks.Select(t => t.ToDto()).ToList();
}
