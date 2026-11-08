using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Reservations;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommandRequest>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentActor _currentActor;

    public CancelReservationCommandHandler(IReservationService reservationService, ICurrentActor currentActor)
    {
        _reservationService = reservationService;
        _currentActor = currentActor;
    }

    public Task Handle(CancelReservationCommandRequest request, CancellationToken cancellationToken)
    {
        return _reservationService.CancelAsync(_currentActor.GetActor(), request, cancellationToken);
    }
}
