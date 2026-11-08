using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Models;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Reservations;

public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQueryRequest, PageResult<ReservationRow>>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentActor _currentActor;

    public GetReservationsQueryHandler(IReservationService reservationService, ICurrentActor currentActor)
    {
        _reservationService = reservationService;
        _currentActor = currentActor;
    }

    public Task<PageResult<ReservationRow>> Handle(GetReservationsQueryRequest request, CancellationToken cancellationToken)
    {
        return _reservationService.GetReservationsAsync(_currentActor.GetActor(), request, cancellationToken);
    }
}
