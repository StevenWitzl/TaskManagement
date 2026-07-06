using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Tasks;

public class GetTasksQueryHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly GetTasksQueryHandler _handler;

    public GetTasksQueryHandlerTests()
    {
        _handler = new GetTasksQueryHandler(_db.Context);
    }

    [Fact]
    public async Task Handle_ReturnsTasksSortedByOrder()
    {
        var user = _db.AddUser();
        _db.AddTask(user.Id, order: 3, title: "C");
        _db.AddTask(user.Id, order: 1, title: "A");
        _db.AddTask(user.Id, order: 2, title: "B");

        var result = await _handler.Handle(new GetTasksQuery(user.Id), CancellationToken.None);

        Assert.Equal(new[] { "A", "B", "C" }, result.Select(t => t.Title));
    }

    [Fact]
    public async Task Handle_ReturnsOpenByOrderThenCompletedByCompletionTime()
    {
        var user = _db.AddUser();
        _db.AddTask(user.Id, order: 2, title: "Open B");
        _db.AddTask(user.Id, order: 5, title: "Done later", completedDate: DateTime.UtcNow);
        _db.AddTask(user.Id, order: 1, title: "Open A");
        _db.AddTask(user.Id, order: 9, title: "Done earlier", completedDate: DateTime.UtcNow.AddHours(-2));

        var result = await _handler.Handle(new GetTasksQuery(user.Id), CancellationToken.None);

        Assert.Equal(new[] { "Open A", "Open B", "Done earlier", "Done later" }, result.Select(t => t.Title));
    }

    [Fact]
    public async Task Handle_ReturnsOnlyOwnTasks()
    {
        var alice = _db.AddUser("alice@test.local");
        var bob = _db.AddUser("bob@test.local");
        _db.AddTask(alice.Id, order: 1, title: "Alice's task");
        _db.AddTask(bob.Id, order: 1, title: "Bob's task");

        var result = await _handler.Handle(new GetTasksQuery(alice.Id), CancellationToken.None);

        var task = Assert.Single(result);
        Assert.Equal("Alice's task", task.Title);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenUserHasNoTasks()
    {
        var user = _db.AddUser();

        var result = await _handler.Handle(new GetTasksQuery(user.Id), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MapsAllFieldsToDto()
    {
        var user = _db.AddUser();
        var completed = DateTime.UtcNow;
        var task = _db.AddTask(user.Id, order: 1, title: "Full", priority: Priority.High, completedDate: completed);

        var result = await _handler.Handle(new GetTasksQuery(user.Id), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(task.Id, dto.Id);
        Assert.Equal(task.Order, dto.Order);
        Assert.Equal(task.Priority, dto.Priority);
        Assert.Equal(task.Title, dto.Title);
        Assert.Equal(task.Description, dto.Description);
        Assert.Equal(task.CompletedDescription, dto.CompletedDescription);
        Assert.NotNull(dto.CompletedDate);
    }

    public void Dispose() => _db.Dispose();
}
