namespace TaskManagement.Api.Application.Common;

/// <summary>
/// Base exception carrying an HTTP status code; translated to a JSON error response by middleware.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, StatusCodes.Status409Conflict)
    {
    }
}

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}
