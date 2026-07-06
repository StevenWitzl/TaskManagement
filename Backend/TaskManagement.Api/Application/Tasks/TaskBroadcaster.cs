using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

/// <summary>
/// Shared helper for command handlers: reads the user's current task list
/// and pushes it to their connected clients via the notifier.
/// </summary>
public static class TaskBroadcaster
{
    public static async Task<List<TaskDto>> BroadcastAsync(
        AppDbContext db,
        ITaskNotifier notifier,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var tasks = await db.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);

        var dtos = TaskOrdering.Sorted(tasks).ToDtos();
        await notifier.BroadcastTasksAsync(userId, dtos, cancellationToken);
        return dtos;
    }
}
