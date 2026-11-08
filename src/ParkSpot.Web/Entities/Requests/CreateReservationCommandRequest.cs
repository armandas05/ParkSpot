using MediatR;
using ParkSpot.Web.Entities.Data;

namespace ParkSpot.Web.Entities.Requests;

public class CreateReservationCommandRequest : IRequest<Reservation>
{
    public required CreateReservationRequest Data { get; set; }
}
