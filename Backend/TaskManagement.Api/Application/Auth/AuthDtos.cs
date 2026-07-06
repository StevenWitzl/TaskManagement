namespace TaskManagement.Api.Application.Auth;

public record RegisterRequestDto(string Email, string Password, string FirstName, string LastName);

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(Guid UserId, string Email, string FirstName, string LastName, string Token);
