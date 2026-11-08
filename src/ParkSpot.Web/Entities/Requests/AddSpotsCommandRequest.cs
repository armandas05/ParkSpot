using MediatR;

namespace ParkSpot.Web.Entities.Requests;

public class AddSpotsCommandRequest : IRequest
{
    public int ParkingLotId { get; set; }
    public required AddSpotsRequest Data { get; set; }
}
