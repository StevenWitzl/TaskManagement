using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Auth;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<AuthResponseDto>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ValidationException("Password must be at least 6 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ValidationException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ValidationException("Last name is required.");
        }

        var exists = await _db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedDate = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registered new user {Email} ({UserId})", user.Email, user.Id);

        return new AuthResponseDto(user.Id, user.Email, user.FirstName, user.LastName, _tokenService.CreateToken(user));
    }
}
