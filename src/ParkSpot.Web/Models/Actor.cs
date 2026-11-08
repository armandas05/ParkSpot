namespace ParkSpot.Web.Models;

public class Actor
{
    public int Id { get; set; }
    public int? OrganizationId { get; set; }
    public bool IsManager { get { return OrganizationId.HasValue; } }
}
