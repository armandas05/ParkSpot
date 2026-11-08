using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Management;

public class CreateLotCommandHandler : IRequestHandler<CreateLotCommandRequest, ParkingLot>
{
    private readonly IManagementService _managementService;
    private readonly ICurrentActor _currentActor;

    public CreateLotCommandHandler(IManagementService managementService, ICurrentActor currentActor)
    {
        _managementService = managementService;
        _currentActor = currentActor;
    }

    public Task<ParkingLot> Handle(CreateLotCommandRequest request, CancellationToken cancellationToken)
    {
        return _managementService.CreateLotAsync(_currentActor.GetActor().OrganizationId!.Value, request, cancellationToken);
    }
}
