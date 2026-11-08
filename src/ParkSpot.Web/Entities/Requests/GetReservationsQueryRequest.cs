using MediatR;
using ParkSpot.Web.Models;

namespace ParkSpot.Web.Entities.Requests;

public class GetReservationsQueryRequest : IRequest<PageResult<ReservationRow>>
{
    public int Page { get; set; }
}
