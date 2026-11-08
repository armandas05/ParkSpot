using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Management;

public class SetAvailabilityCommandHandler : IRequestHandler<SetAvailabilityCommandRequest>
{
    private readonly IManagementService _managementService;
    private readonly ICurrentActor _currentActor;

    public SetAvailabilityCommandHandler(IManagementService managementService, ICurrentActor currentActor)
    {
        _managementService = managementService;
        _currentActor = currentActor;
    }

    public Task Handle(SetAvailabilityCommandRequest request, CancellationToken cancellationToken)
    {
        return _managementService.SetAvailabilityAsync(_currentActor.GetActor().OrganizationId!.Value, request, cancellationToken);
    }
}
