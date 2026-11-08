using MediatR;

namespace ParkSpot.Web.Entities.Requests;

public class SetAvailabilityCommandRequest : IRequest
{
    public int ParkingSpotId { get; set; }
    public required AvailabilityRequest Data { get; set; }
}
