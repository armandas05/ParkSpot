using System;

namespace ParkSpot.Web.Models;

public class ParkingSpotFilter
{
    public int ParkingLotId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Page { get; set; }
    public string? Search { get; set; }
    public bool OnlyAvailable { get; set; }
}
