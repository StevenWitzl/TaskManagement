using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

public record CompleteTaskCommand(Guid UserId, Guid TaskId, string CompletedDescription) : IRequest<TaskDto>;

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
        if (string.IsNullOrWhiteSpace(request.CompletedDescription))
        {
            throw new ValidationException("A completion description is required.");
        }

        var task = await _db.Tasks.FirstOrDefaultAsync(
            t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (task.CompletedDate is not null)
        {
            throw new ConflictException("Task is already completed.");
        }

        task.CompletedDate = DateTime.UtcNow;
        task.CompletedDescription = request.CompletedDescription.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed task {TaskId} for user {UserId}", task.Id, request.UserId);

        await TaskBroadcaster.BroadcastAsync(_db, _notifier, request.UserId, cancellationToken);
        return task.ToDto();
    }
}
