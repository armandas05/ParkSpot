using System;
namespace ParkSpot.Web.Entities.Requests;

public class CreateReservationRequest
{
    public int ParkingSpotId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
}
