using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Services.Interfaces;

namespace ParkSpot.Web.Handlers.Reservations;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommandRequest, Reservation>
{
    private readonly IReservationService _reservationService;
    private readonly ICurrentActor _currentActor;

    public CreateReservationCommandHandler(IReservationService reservationService, ICurrentActor currentActor)
    {
        _reservationService = reservationService;
        _currentActor = currentActor;
    }

    public Task<Reservation> Handle(CreateReservationCommandRequest request, CancellationToken cancellationToken)
    {
        return _reservationService.CreateAsync(_currentActor.GetActor(), request, cancellationToken);
    }
}
