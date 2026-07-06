using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

public record CompleteTaskCommand(Guid UserId, Guid TaskId, string? CompletedDescription) : IRequest<TaskDto>;

public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, TaskDto>
{
    private readonly AppDbContext _db;
    private readonly ITaskNotifier _notifier;
    private readonly ILogger<CompleteTaskCommandHandler> _logger;

    public CompleteTaskCommandHandler(AppDbContext db, ITaskNotifier notifier, ILogger<CompleteTaskCommandHandler> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<TaskDto> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var tasks = await _db.Tasks
            .Where(t => t.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var task = tasks.FirstOrDefault(t => t.Id == request.TaskId);
        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (task.CompletedDate is not null)
        {
            throw new ConflictException("Task is already completed.");
        }

        task.CompletedDate = DateTime.UtcNow;
        task.CompletedDescription = string.IsNullOrWhiteSpace(request.CompletedDescription)
            ? null
            : request.CompletedDescription.Trim();

        // The completed task drops out of the open numbering; keep it contiguous from 1.
        TaskOrdering.RenumberOpen(tasks);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed task {TaskId} for user {UserId}", task.Id, request.UserId);

        await TaskBroadcaster.BroadcastAsync(_db, _notifier, request.UserId, cancellationToken);
        return task.ToDto();
    }
}
