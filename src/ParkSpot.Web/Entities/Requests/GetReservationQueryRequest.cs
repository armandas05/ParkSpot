using System;
using MediatR;
using ParkSpot.Web.Models;

namespace ParkSpot.Web.Entities.Requests;

public class GetReservationQueryRequest : IRequest<ReservationRow>
{
    public Guid ReservationId { get; set; }
}
