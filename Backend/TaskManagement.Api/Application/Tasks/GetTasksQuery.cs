using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Tasks;

public record GetTasksQuery(Guid UserId) : IRequest<List<TaskDto>>;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskDto>>
{
    private readonly AppDbContext _db;

    public GetTasksQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == request.UserId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        return tasks.ToDtos();
    }
}
