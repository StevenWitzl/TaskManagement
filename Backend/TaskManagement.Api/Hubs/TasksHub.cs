using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Application.Tasks;

namespace TaskManagement.Api.Hubs;

/// <summary>
/// Real-time channel for task updates. Each user joins a group keyed by their
/// user id so broadcasts only reach their own connected clients. The current
/// task list is pushed to a client as soon as it connects, so the UI reads
/// everything from this subscription.
/// </summary>
[Authorize]
public class TasksHub : Hub
{
    public static string GroupForUser(Guid userId) => $"user-{userId}";

    private readonly IMediator _mediator;
    private readonly ILogger<TasksHub> _logger;

    public TasksHub(IMediator mediator, ILogger<TasksHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (Guid.TryParse(Context.UserIdentifier, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupForUser(userId));
            _logger.LogInformation("SignalR client {ConnectionId} connected for user {UserId}", Context.ConnectionId, userId);

            var tasks = await _mediator.Send(new GetTasksQuery(userId));
            await Clients.Caller.SendAsync(SignalRTaskNotifier.TasksUpdatedMethod, tasks);
        }

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("SignalR client {ConnectionId} disconnected", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
