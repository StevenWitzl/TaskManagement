using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

public record CreateTaskCommand(Guid UserId, string Title, string Description, Priority Priority)
    : IRequest<TaskDto>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly AppDbContext _db;
    private readonly ITaskNotifier _notifier;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(AppDbContext db, ITaskNotifier notifier, ILogger<CreateTaskCommandHandler> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException("Description is required.");
        }

        var maxOrder = await _db.Tasks
            .Where(t => t.UserId == request.UserId)
            .Select(t => (int?)t.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Order = maxOrder + 1,
            Priority = request.Priority,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created task {TaskId} (order {Order}) for user {UserId}", task.Id, task.Order, request.UserId);

        await TaskBroadcaster.BroadcastAsync(_db, _notifier, request.UserId, cancellationToken);
        return task.ToDto();
    }
}
