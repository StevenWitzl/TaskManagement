using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Hubs;
using Xunit;

namespace TaskManagement.Api.Tests.Hubs;

public class TasksHubTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IGroupManager> _groups = new();
    private readonly Mock<ISingleClientProxy> _caller = new();
    private readonly Mock<IHubCallerClients> _clients = new();
    private readonly Mock<HubCallerContext> _context = new();

    private TasksHub CreateHub(Guid? userId)
    {
        _context.SetupGet(c => c.ConnectionId).Returns("conn-1");
        _context.SetupGet(c => c.UserIdentifier).Returns(userId?.ToString());
        _clients.SetupGet(c => c.Caller).Returns(_caller.Object);

        return new TasksHub(_mediator.Object, NullLogger<TasksHub>.Instance)
        {
            Context = _context.Object,
            Groups = _groups.Object,
            Clients = _clients.Object
        };
    }

    [Fact]
    public async Task OnConnectedAsync_JoinsUserGroupAndPushesCurrentTasks()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskDto>
        {
            new(Guid.NewGuid(), 1, Priority.High, "Task", "Desc", DateTime.UtcNow, null, null)
        };
        _mediator
            .Setup(m => m.Send(It.Is<GetTasksQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        await CreateHub(userId).OnConnectedAsync();

        _groups.Verify(g => g.AddToGroupAsync("conn-1", TasksHub.GroupForUser(userId), It.IsAny<CancellationToken>()), Times.Once);
        _caller.Verify(c => c.SendCoreAsync(
                SignalRTaskNotifier.TasksUpdatedMethod,
                It.Is<object?[]>(args => args.Single() == tasks),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_DoesNothingWithoutUserIdentifier()
    {
        await CreateHub(null).OnConnectedAsync();

        _groups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediator.Verify(m => m.Send(It.IsAny<GetTasksQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
