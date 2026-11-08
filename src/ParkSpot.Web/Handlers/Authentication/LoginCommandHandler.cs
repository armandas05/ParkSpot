using ParkSpot.Web.Entities.Requests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using IAuthenticationService = ParkSpot.Web.Services.Interfaces.IAuthenticationService;

namespace ParkSpot.Web.Handlers.Authentication;

public class LoginCommandHandler : IRequestHandler<LoginCommandRequest>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginCommandHandler(IAuthenticationService authenticationService, IHttpContextAccessor httpContextAccessor)
    {
        _authenticationService = authenticationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(LoginCommandRequest request, CancellationToken cancellationToken)
    {
        var account = await _authenticationService.LoginAsync(request, cancellationToken);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Name),
            new(ClaimTypes.Role, account.Role)
        };

        if (account.OrganizationId.HasValue)
            claims.Add(new Claim("organizationId", account.OrganizationId.Value.ToString()));

        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is unavailable.");

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }
}
