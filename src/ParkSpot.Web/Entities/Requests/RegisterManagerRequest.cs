namespace ParkSpot.Web.Entities.Requests;

public class RegisterManagerRequest : RegisterRequest
{
    public required string OrganizationName { get; set; }
}
