namespace ParkSpot.Web.Entities.Requests;

public class CreateLotRequest
{
    public required string Name { get; set; }
    public required string Address { get; set; }
}
