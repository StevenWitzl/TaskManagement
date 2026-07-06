namespace TaskManagement.Api.Application.Tasks;

/// <summary>
/// Pushes the user's current task list to all of their connected clients.
/// Abstracted from SignalR so command handlers stay unit testable.
/// </summary>
public interface ITaskNotifier
{
    Task BroadcastTasksAsync(Guid userId, IReadOnlyList<TaskDto> tasks, CancellationToken cancellationToken);
}
