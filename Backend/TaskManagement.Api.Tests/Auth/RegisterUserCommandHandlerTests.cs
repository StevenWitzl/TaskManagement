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

    [Fact]
    public async Task Handle_CreatesUserWithHashedPasswordAndReturnsToken()
    {
        var result = await _handler.Handle(new RegisterUserCommand("New@Test.Local", "Secret123!"), CancellationToken.None);

        var user = Assert.Single(_db.Context.Users);
        Assert.Equal("new@test.local", user.Email); // normalized
        Assert.NotEqual("Secret123!", user.PasswordHash); // never stored in plain text
        Assert.True(new Pbkdf2PasswordHasher().Verify("Secret123!", user.PasswordHash));
        Assert.Equal("test-token", result.Token);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenEmailAlreadyRegistered()
    {
        _db.AddUser("taken@test.local");

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new RegisterUserCommand("taken@test.local", "Secret123!"), CancellationToken.None));
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("   ", "Secret123!")]
    public async Task Handle_ThrowsValidation_WhenEmailMissing(string email, string password)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RegisterUserCommand(email, password), CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public async Task Handle_ThrowsValidation_WhenPasswordTooShort(string password)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RegisterUserCommand("new@test.local", password), CancellationToken.None));
    }

    public void Dispose() => _db.Dispose();
}
