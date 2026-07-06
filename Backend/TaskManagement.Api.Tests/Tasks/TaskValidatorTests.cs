using TaskManagement.Api.Application.Tasks;
using TaskManagement.Api.Domain;
using Xunit;

namespace TaskManagement.Api.Tests.Tasks;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    private static CreateTaskCommand Command(
        string title = "Title",
        string description = "Description",
        Priority priority = Priority.Medium) => new(Guid.NewGuid(), title, description, priority);

    [Fact]
    public void Valid_command_passes()
    {
        Assert.True(_validator.Validate(Command()).IsValid);
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("   ", "Description")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    public void Missing_title_or_description_fails(string title, string description)
    {
        Assert.False(_validator.Validate(Command(title: title, description: description)).IsValid);
    }

    [Fact]
    public void Overlong_title_fails()
    {
        Assert.False(_validator.Validate(Command(title: new string('x', 201))).IsValid);
    }

    [Fact]
    public void Undefined_priority_fails()
    {
        Assert.False(_validator.Validate(Command(priority: (Priority)99)).IsValid);
    }
}

public class CompleteTaskCommandValidatorTests
{
    private readonly CompleteTaskCommandValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("done")]
    public void Optional_description_passes(string? description)
    {
        Assert.True(_validator.Validate(new CompleteTaskCommand(Guid.NewGuid(), Guid.NewGuid(), description)).IsValid);
    }

    [Fact]
    public void Overlong_description_fails()
    {
        var command = new CompleteTaskCommand(Guid.NewGuid(), Guid.NewGuid(), new string('x', 2001));
        Assert.False(_validator.Validate(command).IsValid);
    }
}
