using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Auth;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Auth;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly Pbkdf2PasswordHasher _hasher = new();
    private readonly Mock<IJwtTokenService> _tokenService = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _tokenService.Setup(s => s.CreateToken(It.IsAny<User>())).Returns("test-token");
        _handler = new LoginCommandHandler(
            _db.Context,
            _hasher,
            _tokenService.Object,
            NullLogger<LoginCommandHandler>.Instance);
    }

    private User AddUserWithPassword(string email, string password)
    {
        var user = _db.AddUser(email);
        user.PasswordHash = _hasher.Hash(password);
        _db.Context.SaveChanges();
        return user;
    }

    [Fact]
    public async Task Handle_ReturnsToken_ForValidCredentials()
    {
        var user = AddUserWithPassword("user@test.local", "Secret123!");

        var result = await _handler.Handle(new LoginCommand("user@test.local", "Secret123!"), CancellationToken.None);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("user@test.local", result.Email);
        Assert.Equal("test-token", result.Token);
    }

    [Fact]
    public async Task Handle_IsCaseInsensitiveOnEmail()
    {
        AddUserWithPassword("user@test.local", "Secret123!");

        var result = await _handler.Handle(new LoginCommand("USER@Test.Local", "Secret123!"), CancellationToken.None);

        Assert.Equal("user@test.local", result.Email);
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_ForWrongPassword()
    {
        AddUserWithPassword("user@test.local", "Secret123!");

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(new LoginCommand("user@test.local", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_ForUnknownEmail()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(new LoginCommand("nobody@test.local", "Secret123!"), CancellationToken.None));
    }

    public void Dispose() => _db.Dispose();
}
