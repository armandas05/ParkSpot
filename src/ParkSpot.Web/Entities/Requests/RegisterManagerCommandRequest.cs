using MediatR;

namespace ParkSpot.Web.Entities.Requests;

public class RegisterManagerCommandRequest : IRequest<int>
{
    public required RegisterManagerRequest Data { get; set; }
}
