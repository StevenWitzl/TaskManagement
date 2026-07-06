using FluentValidation;
using MediatR;
using TaskManagement.Api.Application.Behaviors;
using Xunit;
using ValidationException = TaskManagement.Api.Application.Common.ValidationException;

namespace TaskManagement.Api.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<string>;

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(r => r.Value).NotEmpty().WithMessage("Value is required.");
        }
    }

    private static Task<string> Next() => Task.FromResult("handled");

    [Fact]
    public async Task Handle_InvokesHandler_WhenRequestIsValid()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { new TestRequestValidator() });

        var result = await behavior.Handle(new TestRequest("ok"), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenRequestIsInvalid()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { new TestRequestValidator() });

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), Next, CancellationToken.None));

        Assert.Equal("Value is required.", ex.Message);
    }

    [Fact]
    public async Task Handle_InvokesHandler_WhenNoValidatorsRegistered()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());

        var result = await behavior.Handle(new TestRequest(""), Next, CancellationToken.None);

        Assert.Equal("handled", result);
    }
}
