using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Models;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Reservations;

public class GetReservationQueryHandler : IRequestHandler<GetReservationQueryRequest, ReservationRow>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentActor _currentActor;

    public GetReservationQueryHandler(IReservationService reservationService, ICurrentActor currentActor)
    {
        _reservationService = reservationService;
        _currentActor = currentActor;
    }

    public Task<ReservationRow> Handle(GetReservationQueryRequest request, CancellationToken cancellationToken)
    {
        return _reservationService.GetReservationAsync(_currentActor.GetActor(), request, cancellationToken);
    }
}
