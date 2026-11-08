namespace ParkSpot.Web.Entities.Requests;

public class ReserveNowRequest
{
    public int ParkingSpotId { get; set; }
    public int DurationMinutes { get; set; }
}
