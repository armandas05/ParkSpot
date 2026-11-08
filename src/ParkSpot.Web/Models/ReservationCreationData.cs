using System;

namespace ParkSpot.Web.Models;

public class ReservationCreationData
{
    public int ParkingSpotId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public DateTime RequestedAt { get; set; }
}
