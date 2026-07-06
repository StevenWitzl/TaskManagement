using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Tasks;

public class CreateTaskCommandHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly Mock<ITaskNotifier> _notifier = new();
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _handler = new CreateTaskCommandHandler(
            _db.Context, _notifier.Object, NullLogger<CreateTaskCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_AssignsOrderOne_WhenUserHasNoTasks()
    {
        var user = _db.AddUser();

        var result = await _handler.Handle(
            new CreateTaskCommand(user.Id, "First", "Description", Priority.High), CancellationToken.None);

        Assert.Equal(1, result.Order);
        Assert.Equal(Priority.High, result.Priority);
        Assert.NotEqual(default, result.CreatedDate);
        Assert.Null(result.CompletedDate);
        Assert.Null(result.CompletedDescription);
    }

    [Fact]
    public async Task Handle_AppendsAfterHighestExistingOrder()
    {
        var user = _db.AddUser();
        _db.AddTask(user.Id, order: 1);
        _db.AddTask(user.Id, order: 2);

        var result = await _handler.Handle(
            new CreateTaskCommand(user.Id, "Third", "Description", Priority.Low), CancellationToken.None);

        Assert.Equal(3, result.Order);
    }

    [Fact]
    public async Task Handle_OrderIsPerUser()
    {
        var alice = _db.AddUser("alice@test.local");
        var bob = _db.AddUser("bob@test.local");
        _db.AddTask(alice.Id, order: 1);
        _db.AddTask(alice.Id, order: 2);

        var result = await _handler.Handle(
            new CreateTaskCommand(bob.Id, "Bob's first", "Description", Priority.Medium), CancellationToken.None);

        Assert.Equal(1, result.Order);
    }

    [Fact]
    public async Task Handle_TrimsTitleAndDescription()
    {
        var user = _db.AddUser();

        var result = await _handler.Handle(
            new CreateTaskCommand(user.Id, "  Padded  ", "  Also padded  ", Priority.Medium), CancellationToken.None);

        Assert.Equal("Padded", result.Title);
        Assert.Equal("Also padded", result.Description);
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("   ", "Description")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    public async Task Handle_ThrowsValidation_WhenTitleOrDescriptionMissing(string title, string description)
    {
        var user = _db.AddUser();

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new CreateTaskCommand(user.Id, title, description, Priority.Medium), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BroadcastsUpdatedTaskListToOwner()
    {
        var user = _db.AddUser();

        await _handler.Handle(
            new CreateTaskCommand(user.Id, "First", "Description", Priority.High), CancellationToken.None);

        _notifier.Verify(n => n.BroadcastTasksAsync(
                user.Id,
                It.Is<IReadOnlyList<TaskDto>>(tasks => tasks.Count == 1 && tasks[0].Title == "First"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose() => _db.Dispose();
}
