using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

public record ReorderTaskCommand(Guid UserId, Guid TaskId, int NewOrder) : IRequest<List<TaskDto>>;

public class ReorderTaskCommandHandler : IRequestHandler<ReorderTaskCommand, List<TaskDto>>
{
    private readonly AppDbContext _db;
    private readonly ITaskNotifier _notifier;
    private readonly ILogger<ReorderTaskCommandHandler> _logger;

    public ReorderTaskCommandHandler(AppDbContext db, ITaskNotifier notifier, ILogger<ReorderTaskCommandHandler> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<List<TaskDto>> Handle(ReorderTaskCommand request, CancellationToken cancellationToken)
    {
        // Only open tasks participate in ordering.
        var tasks = await _db.Tasks
            .Where(t => t.UserId == request.UserId && t.CompletedDate == null)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        var task = tasks.FirstOrDefault(t => t.Id == request.TaskId);
        if (task is null)
        {
            throw new NotFoundException("Task not found or already completed.");
        }

        // Move the task to the requested position (1-based, clamped), then renumber sequentially.
        var newIndex = Math.Clamp(request.NewOrder, 1, tasks.Count) - 1;
        tasks.Remove(task);
        tasks.Insert(newIndex, task);

        for (var i = 0; i < tasks.Count; i++)
        {
            tasks[i].Order = i + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Moved task {TaskId} to position {Order} for user {UserId}", task.Id, task.Order, request.UserId);

        return await TaskBroadcaster.BroadcastAsync(_db, _notifier, request.UserId, cancellationToken);
    }
}
