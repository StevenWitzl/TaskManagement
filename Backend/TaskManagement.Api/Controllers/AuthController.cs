using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Application.Auth;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request.Email, request.Password), cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(result);
    }
}
