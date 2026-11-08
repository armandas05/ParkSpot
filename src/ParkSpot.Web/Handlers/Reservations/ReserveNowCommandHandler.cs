using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Reservations;

public class ReserveNowCommandHandler : IRequestHandler<ReserveNowCommandRequest, Reservation>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentActor _currentActor;

    public ReserveNowCommandHandler(IReservationService reservationService, ICurrentActor currentActor)
    {
        _reservationService = reservationService;
        _currentActor = currentActor;
    }

    public Task<Reservation> Handle(ReserveNowCommandRequest request, CancellationToken cancellationToken)
    {
        return _reservationService.CreateNowAsync(_currentActor.GetActor(), request, cancellationToken);
    }
}
