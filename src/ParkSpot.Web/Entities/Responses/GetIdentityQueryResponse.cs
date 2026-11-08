using MediatR;

namespace ParkSpot.Web.Entities.Responses;

public class GetIdentityQueryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public int? OrganizationId { get; set; }
}
