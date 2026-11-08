namespace ParkSpot.Web.Models;

public class ParkingSpotRow
{
    public int Id { get; set; }
    public required string Number { get; set; }
    public bool IsReservable { get; set; }
    public bool IsAvailable { get; set; }
}
