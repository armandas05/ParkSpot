using ParkSpot.Web.Entities.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace ParkSpot.Web.Handlers.Authentication;

public class LogoutCommandHandler : IRequestHandler<LogoutCommandRequest>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task Handle(LogoutCommandRequest request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is unavailable.");

        return httpContext.SignOutAsync();
    }
}
