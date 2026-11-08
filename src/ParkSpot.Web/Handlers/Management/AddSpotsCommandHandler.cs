using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Management;

public class AddSpotsCommandHandler : IRequestHandler<AddSpotsCommandRequest>
{
    private readonly IManagementService _managementService;
    private readonly ICurrentActor _currentActor;

    public AddSpotsCommandHandler(IManagementService managementService, ICurrentActor currentActor)
    {
        _managementService = managementService;
        _currentActor = currentActor;
    }

    public Task Handle(AddSpotsCommandRequest request, CancellationToken cancellationToken)
    {
        return _managementService.AddSpotsAsync(_currentActor.GetActor().OrganizationId!.Value, request, cancellationToken);
    }
}
