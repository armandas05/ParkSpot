using System;
namespace ParkSpot.Web.Entities.Data;

public class ParkingLot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
