using System;

namespace ParkSpot.Web.Models;

public class ReservationOverlapFilter
{
    public int ParkingSpotId { get; set; }
    public int ParkingLotId { get; set; }
    public int UserId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
