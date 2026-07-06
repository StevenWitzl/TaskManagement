using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

public record DeleteTaskCommand(Guid UserId, Guid TaskId) : IRequest<Unit>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly AppDbContext _db;
    private readonly ITaskNotifier _notifier;
    private readonly ILogger<DeleteTaskCommandHandler> _logger;

    public DeleteTaskCommandHandler(AppDbContext db, ITaskNotifier notifier, ILogger<DeleteTaskCommandHandler> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(
            t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }

        _db.Tasks.Remove(task);

        // Keep Order values contiguous after a delete.
        var remaining = await _db.Tasks
            .Where(t => t.UserId == request.UserId && t.Id != request.TaskId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < remaining.Count; i++)
        {
            remaining[i].Order = i + 1;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted task {TaskId} for user {UserId}", request.TaskId, request.UserId);

        await TaskBroadcaster.BroadcastAsync(_db, _notifier, request.UserId, cancellationToken);
        return Unit.Value;
    }
}
