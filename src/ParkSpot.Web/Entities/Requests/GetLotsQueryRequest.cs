using System.Collections.Generic;
using MediatR;
using ParkSpot.Web.Models;

namespace ParkSpot.Web.Entities.Requests;

public class GetLotsQueryRequest : IRequest<IReadOnlyList<ParkingLotRow>>
{
}
