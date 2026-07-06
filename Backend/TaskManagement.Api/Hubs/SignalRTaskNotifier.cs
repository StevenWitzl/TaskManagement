using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Application.Tasks;

namespace TaskManagement.Api.Hubs;

public class SignalRTaskNotifier : ITaskNotifier
{
    public const string TasksUpdatedMethod = "TasksUpdated";

    private readonly IHubContext<TasksHub> _hubContext;

    public SignalRTaskNotifier(IHubContext<TasksHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastTasksAsync(Guid userId, IReadOnlyList<TaskDto> tasks, CancellationToken cancellationToken)
    {
        return _hubContext.Clients
            .Group(TasksHub.GroupForUser(userId))
            .SendAsync(TasksUpdatedMethod, tasks, cancellationToken);
    }
}
