using FluentValidation;
using MediatR;

namespace TaskManagement.Api.Application.Behaviors;

/// <summary>
/// Runs every registered validator for the request before it reaches its handler,
/// so validation stays a cross-cutting concern instead of if-statements in handlers.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

            if (failures.Count > 0)
            {
                throw new Common.ValidationException(string.Join(" ", failures.Select(f => f.ErrorMessage)));
            }
        }

        return await next();
    }
}
