using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Models;
using ParkSpot.Web.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace ParkSpot.Web.Services;

public class CurrentActor : ICurrentActor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentActor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public AccountIdentity? GetIdentity()
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        return new AccountIdentity
        {
            Id = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Name = principal.Identity.Name ?? "",
            Role = principal.FindFirstValue(ClaimTypes.Role)!,
            OrganizationId = principal.IsInRole("Manager") ? int.Parse(principal.FindFirstValue("organizationId")!) : null
        };
    }

    public Actor GetActor()
    {
        var identity = GetIdentity() ?? throw new DomainException("Prisijunkite.", 401);

        return new Actor
        {
            Id = identity.Id,
            OrganizationId = identity.OrganizationId
        };
    }
}
