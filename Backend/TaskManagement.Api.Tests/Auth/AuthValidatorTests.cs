using TaskManagement.Api.Application.Auth;
using Xunit;

namespace TaskManagement.Api.Tests.Auth;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private static RegisterUserCommand Command(
        string email = "new@test.local",
        string password = "Secret123!",
        string firstName = "Ada",
        string lastName = "Lovelace") => new(email, password, firstName, lastName);

    [Fact]
    public void Valid_command_passes()
    {
        Assert.True(_validator.Validate(Command()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        Assert.False(_validator.Validate(Command(email: email)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Short_password_fails(string password)
    {
        Assert.False(_validator.Validate(Command(password: password)).IsValid);
    }

    [Theory]
    [InlineData("", "Lovelace")]
    [InlineData("   ", "Lovelace")]
    [InlineData("Ada", "")]
    [InlineData("Ada", "   ")]
    public void Missing_name_fails(string firstName, string lastName)
    {
        Assert.False(_validator.Validate(Command(firstName: firstName, lastName: lastName)).IsValid);
    }
}

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        Assert.True(_validator.Validate(new LoginCommand("user@test.local", "Secret123!")).IsValid);
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("user@test.local", "")]
    public void Missing_credentials_fail(string email, string password)
    {
        Assert.False(_validator.Validate(new LoginCommand(email, password)).IsValid);
    }
}
