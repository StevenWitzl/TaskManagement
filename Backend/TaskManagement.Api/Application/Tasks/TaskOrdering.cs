using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Application.Tasks;

/// <summary>
/// Order only applies to open tasks and is always contiguous from 1.
/// Completed tasks drop out of the numbering and sort by completion time.
/// </summary>
public static class TaskOrdering
{
    public static List<TaskItem> Sorted(IEnumerable<TaskItem> tasks)
    {
        var all = tasks.ToList();
        return all.Where(t => t.CompletedDate == null).OrderBy(t => t.Order)
            .Concat(all.Where(t => t.CompletedDate != null).OrderBy(t => t.CompletedDate))
            .ToList();
    }

    public static void RenumberOpen(IEnumerable<TaskItem> tasks)
    {
        var open = tasks.Where(t => t.CompletedDate == null).OrderBy(t => t.Order).ToList();
        for (var i = 0; i < open.Count; i++)
        {
            open[i].Order = i + 1;
        }
    }
}
