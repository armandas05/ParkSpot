using MediatR;

namespace ParkSpot.Web.Entities.Requests;

public class LoginCommandRequest : IRequest
{
    public required string AccountType { get; set; }
    public required LoginRequest Data { get; set; }
}
