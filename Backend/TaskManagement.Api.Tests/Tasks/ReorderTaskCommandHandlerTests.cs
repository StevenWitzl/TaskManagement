using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Tasks;

public class ReorderTaskCommandHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly Mock<ITaskNotifier> _notifier = new();
    private readonly ReorderTaskCommandHandler _handler;

    public ReorderTaskCommandHandlerTests()
    {
        _handler = new ReorderTaskCommandHandler(
            _db.Context, _notifier.Object, NullLogger<ReorderTaskCommandHandler>.Instance);
    }

    private (TaskItem a, TaskItem b, TaskItem c) SeedThreeTasks(Guid userId) => (
        _db.AddTask(userId, order: 1, title: "A"),
        _db.AddTask(userId, order: 2, title: "B"),
        _db.AddTask(userId, order: 3, title: "C"));

    [Fact]
    public async Task Handle_MovesTaskDownAndShiftsOthersUp()
    {
        var user = _db.AddUser();
        var (a, _, _) = SeedThreeTasks(user.Id);

        var result = await _handler.Handle(new ReorderTaskCommand(user.Id, a.Id, 3), CancellationToken.None);

        Assert.Equal(new[] { "B", "C", "A" }, result.Select(t => t.Title));
        Assert.Equal(new[] { 1, 2, 3 }, result.Select(t => t.Order));
    }

    [Fact]
    public async Task Handle_MovesTaskUpAndShiftsOthersDown()
    {
        var user = _db.AddUser();
        var (_, _, c) = SeedThreeTasks(user.Id);

        var result = await _handler.Handle(new ReorderTaskCommand(user.Id, c.Id, 1), CancellationToken.None);

        Assert.Equal(new[] { "C", "A", "B" }, result.Select(t => t.Title));
        Assert.Equal(new[] { 1, 2, 3 }, result.Select(t => t.Order));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Handle_ClampsOrderBelowRangeToFirst(int requestedOrder)
    {
        var user = _db.AddUser();
        var (_, b, _) = SeedThreeTasks(user.Id);

        var result = await _handler.Handle(new ReorderTaskCommand(user.Id, b.Id, requestedOrder), CancellationToken.None);

        Assert.Equal("B", result.First().Title);
    }

    [Fact]
    public async Task Handle_ClampsOrderAboveRangeToLast()
    {
        var user = _db.AddUser();
        var (a, _, _) = SeedThreeTasks(user.Id);

        var result = await _handler.Handle(new ReorderTaskCommand(user.Id, a.Id, 99), CancellationToken.None);

        Assert.Equal("A", result.Last().Title);
        Assert.Equal(3, result.Last().Order);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTaskBelongsToAnotherUser()
    {
        var owner = _db.AddUser("owner@test.local");
        var intruder = _db.AddUser("intruder@test.local");
        var task = _db.AddTask(owner.Id, order: 1);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new ReorderTaskCommand(intruder.Id, task.Id, 1), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DoesNotTouchOtherUsersOrdering()
    {
        var alice = _db.AddUser("alice@test.local");
        var bob = _db.AddUser("bob@test.local");
        var aliceTask = _db.AddTask(alice.Id, order: 1, title: "Alice 1");
        _db.AddTask(alice.Id, order: 2, title: "Alice 2");
        var bobTask = _db.AddTask(bob.Id, order: 1, title: "Bob 1");

        await _handler.Handle(new ReorderTaskCommand(alice.Id, aliceTask.Id, 2), CancellationToken.None);

        _db.Context.Entry(bobTask).Reload();
        Assert.Equal(1, bobTask.Order);
    }

    public void Dispose() => _db.Dispose();
}
