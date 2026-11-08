using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Models;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Parking;

public class GetSpotsQueryHandler : IRequestHandler<GetSpotsQueryRequest, PageResult<ParkingSpotRow>>
{
    private readonly IParkingService _parkingService;
    private readonly ICurrentActor _currentActor;

    public GetSpotsQueryHandler(IParkingService parkingService, ICurrentActor currentActor)
    {
        _parkingService = parkingService;
        _currentActor = currentActor;
    }

    public Task<PageResult<ParkingSpotRow>> Handle(GetSpotsQueryRequest request, CancellationToken cancellationToken)
    {
        return _parkingService.GetSpotsAsync(_currentActor.GetActor(), request, cancellationToken);
    }
}
