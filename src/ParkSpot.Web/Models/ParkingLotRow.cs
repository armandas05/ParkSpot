namespace ParkSpot.Web.Models;

public class ParkingLotRow
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public int OrganizationId { get; set; }
}
