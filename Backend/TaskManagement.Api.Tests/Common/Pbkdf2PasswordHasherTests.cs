using TaskManagement.Api.Application.Common;
using Xunit;

namespace TaskManagement.Api.Tests.Common;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_ReturnsTrueForCorrectPassword()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.True(_hasher.Verify("Secret123!", hash));
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword()
    {
        // Unique salt per hash
        Assert.NotEqual(_hasher.Hash("Secret123!"), _hasher.Hash("Secret123!"));
    }

    [Fact]
    public void Verify_ReturnsFalseForMalformedHash()
    {
        Assert.False(_hasher.Verify("Secret123!", "not-a-valid-hash"));
    }
}
