using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Tasks;

public class CompleteTaskCommandHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly Mock<ITaskNotifier> _notifier = new();
    private readonly CompleteTaskCommandHandler _handler;

    public CompleteTaskCommandHandlerTests()
    {
        _handler = new CompleteTaskCommandHandler(
            _db.Context, _notifier.Object, NullLogger<CompleteTaskCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_SetsCompletedDateAndDescription()
    {
        var user = _db.AddUser();
        var task = _db.AddTask(user.Id, order: 1);

        var result = await _handler.Handle(
            new CompleteTaskCommand(user.Id, task.Id, "All done"), CancellationToken.None);

        Assert.NotNull(result.CompletedDate);
        Assert.Equal("All done", result.CompletedDescription);
        Assert.InRange(result.CompletedDate!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTaskDoesNotExist()
    {
        var user = _db.AddUser();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CompleteTaskCommand(user.Id, Guid.NewGuid(), "done"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTaskBelongsToAnotherUser()
    {
        var owner = _db.AddUser("owner@test.local");
        var intruder = _db.AddUser("intruder@test.local");
        var task = _db.AddTask(owner.Id, order: 1);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CompleteTaskCommand(intruder.Id, task.Id, "done"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenTaskAlreadyCompleted()
    {
        var user = _db.AddUser();
        var task = _db.AddTask(user.Id, order: 1, completedDate: DateTime.UtcNow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new CompleteTaskCommand(user.Id, task.Id, "again"), CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_AllowsMissingCompletionDescription(string? description)
    {
        var user = _db.AddUser();
        var task = _db.AddTask(user.Id, order: 1);

        var result = await _handler.Handle(
            new CompleteTaskCommand(user.Id, task.Id, description), CancellationToken.None);

        Assert.NotNull(result.CompletedDate);
        Assert.Null(result.CompletedDescription); // blank input normalizes to null
    }

    [Fact]
    public async Task Handle_RenumbersRemainingOpenTasksFromOne()
    {
        var user = _db.AddUser();
        var first = _db.AddTask(user.Id, order: 1, title: "A");
        var second = _db.AddTask(user.Id, order: 2, title: "B");
        var third = _db.AddTask(user.Id, order: 3, title: "C");

        await _handler.Handle(new CompleteTaskCommand(user.Id, first.Id, "done"), CancellationToken.None);

        _db.Context.Entry(second).Reload();
        _db.Context.Entry(third).Reload();
        Assert.Equal(1, second.Order); // open numbering always starts at 1
        Assert.Equal(2, third.Order);
    }

    [Fact]
    public async Task Handle_BroadcastsUpdatedTaskList()
    {
        var user = _db.AddUser();
        var task = _db.AddTask(user.Id, order: 1);

        await _handler.Handle(new CompleteTaskCommand(user.Id, task.Id, "done"), CancellationToken.None);

        _notifier.Verify(n => n.BroadcastTasksAsync(
                user.Id,
                It.Is<IReadOnlyList<TaskDto>>(tasks => tasks.Single().CompletedDate != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose() => _db.Dispose();
}
