using TaskManagement.Api.Domain;

namespace TaskManagement.Api.Application.Auth;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
