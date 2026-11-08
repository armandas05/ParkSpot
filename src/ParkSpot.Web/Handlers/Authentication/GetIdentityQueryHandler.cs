using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Entities.Responses;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Authentication;

public class GetIdentityQueryHandler : IRequestHandler<GetIdentityQueryRequest, GetIdentityQueryResponse?>
{
    private readonly ICurrentActor _currentActor;

    public GetIdentityQueryHandler(ICurrentActor currentActor)
    {
        _currentActor = currentActor;
    }

    public Task<GetIdentityQueryResponse?> Handle(GetIdentityQueryRequest request, CancellationToken cancellationToken)
    {
        var identity = _currentActor.GetIdentity();

        if (identity is null)
            return Task.FromResult<GetIdentityQueryResponse?>(null);

        var response = new GetIdentityQueryResponse
        {
            Id = identity.Id,
            Name = identity.Name,
            Role = identity.Role,
            OrganizationId = identity.OrganizationId,
        };

        return Task.FromResult<GetIdentityQueryResponse?>(response);
    }
}
