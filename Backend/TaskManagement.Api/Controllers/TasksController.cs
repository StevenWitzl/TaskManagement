using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Application.Tasks;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetTasks(CancellationToken cancellationToken)
    {
        var tasks = await _mediator.Send(new GetTasksQuery(CurrentUserId), cancellationToken);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskRequestDto request, CancellationToken cancellationToken)
    {
        var task = await _mediator.Send(
            new CreateTaskCommand(CurrentUserId, request.Title, request.Description, request.Priority),
            cancellationToken);
        return Ok(task);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<TaskDto>> CompleteTask(Guid id, CompleteTaskRequestDto request, CancellationToken cancellationToken)
    {
        var task = await _mediator.Send(
            new CompleteTaskCommand(CurrentUserId, id, request.CompletedDescription),
            cancellationToken);
        return Ok(task);
    }

    [HttpPost("{id:guid}/reorder")]
    public async Task<ActionResult<List<TaskDto>>> ReorderTask(Guid id, ReorderTaskRequestDto request, CancellationToken cancellationToken)
    {
        var tasks = await _mediator.Send(
            new ReorderTaskCommand(CurrentUserId, id, request.NewOrder),
            cancellationToken);
        return Ok(tasks);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTaskCommand(CurrentUserId, id), cancellationToken);
        return NoContent();
    }
}
