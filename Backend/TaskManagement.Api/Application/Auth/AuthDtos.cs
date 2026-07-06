namespace TaskManagement.Api.Application.Auth;

public record RegisterRequestDto(string Email, string Password);

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(Guid UserId, string Email, string Token);
