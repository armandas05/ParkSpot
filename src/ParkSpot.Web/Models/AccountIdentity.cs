namespace ParkSpot.Web.Models;

public class AccountIdentity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public int? OrganizationId { get; set; }
}
