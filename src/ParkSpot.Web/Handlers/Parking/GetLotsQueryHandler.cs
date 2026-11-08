using ParkSpot.Web.Entities.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Models;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Parking;

public class GetLotsQueryHandler : IRequestHandler<GetLotsQueryRequest, IReadOnlyList<ParkingLotRow>>
{
    private readonly IParkingService _parkingService;
    private readonly ICurrentActor _currentActor;

    public GetLotsQueryHandler(IParkingService parkingService, ICurrentActor currentActor)
    {
        _parkingService = parkingService;
        _currentActor = currentActor;
    }

    public Task<IReadOnlyList<ParkingLotRow>> Handle(GetLotsQueryRequest request, CancellationToken cancellationToken)
    {
        return _parkingService.GetLotsAsync(_currentActor.GetActor(), cancellationToken);
    }
}
