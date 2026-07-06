using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Infrastructure;

namespace TaskManagement.Api.Application.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        _logger.LogInformation("User {Email} ({UserId}) signed in", user.Email, user.Id);

        return new AuthResponseDto(user.Id, user.Email, user.FirstName, user.LastName, _tokenService.CreateToken(user));
    }
}
