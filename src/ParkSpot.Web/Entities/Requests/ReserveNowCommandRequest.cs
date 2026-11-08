using MediatR;
using ParkSpot.Web.Entities.Data;

namespace ParkSpot.Web.Entities.Requests;

public class ReserveNowCommandRequest : IRequest<Reservation>
{
    public required ReserveNowRequest Data { get; set; }
}
