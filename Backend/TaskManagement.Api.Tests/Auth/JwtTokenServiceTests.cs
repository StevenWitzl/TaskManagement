using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagement.Api.Application.Auth;
using TaskManagement.Api.Domain;
using Xunit;

namespace TaskManagement.Api.Tests.Auth;

public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Key = "unit-test-signing-key-0123456789-0123456789",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpiryMinutes = 60
    };

    [Fact]
    public void CreateToken_EmbedsUserIdEmailIssuerAndAudience()
    {
        var service = new JwtTokenService(Settings);
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.local" };

        var token = service.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(Settings.Issuer, jwt.Issuer);
        Assert.Equal(Settings.Audience, jwt.Audiences.Single());
    }

    [Fact]
    public void CreateToken_SetsExpiryFromSettings()
    {
        var service = new JwtTokenService(Settings);
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.local" };

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(service.CreateToken(user));

        var expected = DateTime.UtcNow.AddMinutes(Settings.ExpiryMinutes);
        Assert.InRange(jwt.ValidTo, expected.AddMinutes(-1), expected.AddMinutes(1));
    }
}
