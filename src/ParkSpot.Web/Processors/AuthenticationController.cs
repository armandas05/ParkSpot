using MediatR;
using ParkSpot.Web.Entities.Requests;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ParkSpot.Web.Processors;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetIdentity(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetIdentityQueryRequest(), cancellationToken));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(new RegisterCommandRequest
        {
            Data = request
        }, cancellationToken);

        return Created("/api/auth/me", new
        {
            id = userId
        });
    }

    [HttpPost("register/manager")]
    public async Task<IActionResult> RegisterManager(RegisterManagerRequest request, CancellationToken cancellationToken)
    {
        var managerId = await _mediator.Send(new RegisterManagerCommandRequest
        {
            Data = request
        }, cancellationToken);

        return Created("/api/auth/me", new
        {
            id = managerId
        });
    }

    [HttpPost("login/{accountType}")]
    public async Task<IActionResult> Login(string accountType, LoginRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LoginCommandRequest
        {
            AccountType = accountType,
            Data = request
        }, cancellationToken);

        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _mediator.Send(new LogoutCommandRequest(), cancellationToken);

        return NoContent();
    }
}
