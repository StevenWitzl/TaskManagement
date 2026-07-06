using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Tasks;

public class DeleteTaskCommandHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly Mock<ITaskNotifier> _notifier = new();
    private readonly DeleteTaskCommandHandler _handler;

    public DeleteTaskCommandHandlerTests()
    {
        _handler = new DeleteTaskCommandHandler(
            _db.Context, _notifier.Object, NullLogger<DeleteTaskCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_RemovesTaskAndRenumbersRemaining()
    {
        var user = _db.AddUser();
        _db.AddTask(user.Id, order: 1, title: "A");
        var middle = _db.AddTask(user.Id, order: 2, title: "B");
        _db.AddTask(user.Id, order: 3, title: "C");

        await _handler.Handle(new DeleteTaskCommand(user.Id, middle.Id), CancellationToken.None);

        var remaining = _db.Context.Tasks.OrderBy(t => t.Order).ToList();
        Assert.Equal(new[] { "A", "C" }, remaining.Select(t => t.Title));
        Assert.Equal(new[] { 1, 2 }, remaining.Select(t => t.Order));
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTaskDoesNotExist()
    {
        var user = _db.AddUser();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteTaskCommand(user.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTaskBelongsToAnotherUser()
    {
        var owner = _db.AddUser("owner@test.local");
        var intruder = _db.AddUser("intruder@test.local");
        var task = _db.AddTask(owner.Id, order: 1);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteTaskCommand(intruder.Id, task.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BroadcastsUpdatedTaskList()
    {
        var user = _db.AddUser();
        var task = _db.AddTask(user.Id, order: 1);

        await _handler.Handle(new DeleteTaskCommand(user.Id, task.Id), CancellationToken.None);

        _notifier.Verify(n => n.BroadcastTasksAsync(
                user.Id,
                It.Is<IReadOnlyList<TaskDto>>(tasks => tasks.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose() => _db.Dispose();
}
