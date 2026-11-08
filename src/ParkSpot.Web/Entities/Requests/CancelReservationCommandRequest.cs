using System;
using MediatR;

namespace ParkSpot.Web.Entities.Requests;

public class CancelReservationCommandRequest : IRequest
{
    public Guid ReservationId { get; set; }
}
