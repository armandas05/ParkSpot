using MediatR;

namespace ParkSpot.Web.Entities.Requests;

public class RegisterCommandRequest : IRequest<int>
{
    public required RegisterRequest Data { get; set; }
}
