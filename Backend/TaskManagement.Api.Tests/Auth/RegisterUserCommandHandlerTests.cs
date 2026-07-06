using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Api.Application.Auth;
using TaskManagement.Api.Application.Common;
using TaskManagement.Api.Domain;
using TaskManagement.Api.Tests.TestHelpers;
using Xunit;

namespace TaskManagement.Api.Tests.Auth;

public class RegisterUserCommandHandlerTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly Mock<IJwtTokenService> _tokenService = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _tokenService.Setup(s => s.CreateToken(It.IsAny<User>())).Returns("test-token");
        _handler = new RegisterUserCommandHandler(
            _db.Context,
            new Pbkdf2PasswordHasher(),
            _tokenService.Object,
            NullLogger<RegisterUserCommandHandler>.Instance);
    }

    private static RegisterUserCommand Command(
        string email = "new@test.local",
        string password = "Secret123!",
        string firstName = "Ada",
        string lastName = "Lovelace") => new(email, password, firstName, lastName);

    [Fact]
    public async Task Handle_CreatesUserWithHashedPasswordAndReturnsToken()
    {
        var result = await _handler.Handle(Command(email: "New@Test.Local"), CancellationToken.None);

        var user = Assert.Single(_db.Context.Users);
        Assert.Equal("new@test.local", user.Email); // normalized
        Assert.Equal("Ada", user.FirstName);
        Assert.Equal("Lovelace", user.LastName);
        Assert.NotEqual("Secret123!", user.PasswordHash); // never stored in plain text
        Assert.True(new Pbkdf2PasswordHasher().Verify("Secret123!", user.PasswordHash));
        Assert.Equal("test-token", result.Token);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Ada", result.FirstName);
        Assert.Equal("Lovelace", result.LastName);
    }

    [Fact]
    public async Task Handle_TrimsNames()
    {
        await _handler.Handle(Command(firstName: "  Ada  ", lastName: "  Lovelace  "), CancellationToken.None);

        var user = Assert.Single(_db.Context.Users);
        Assert.Equal("Ada", user.FirstName);
        Assert.Equal("Lovelace", user.LastName);
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenEmailAlreadyRegistered()
    {
        _db.AddUser("taken@test.local");

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(Command(email: "taken@test.local"), CancellationToken.None));
    }

    public void Dispose() => _db.Dispose();
}
