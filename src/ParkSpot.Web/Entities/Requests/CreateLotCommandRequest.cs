using MediatR;
using ParkSpot.Web.Entities.Data;

namespace ParkSpot.Web.Entities.Requests;

public class CreateLotCommandRequest : IRequest<ParkingLot>
{
    public required CreateLotRequest Data { get; set; }
}
