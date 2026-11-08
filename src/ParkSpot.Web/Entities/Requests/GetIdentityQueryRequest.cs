using MediatR;
using ParkSpot.Web.Entities.Responses;

namespace ParkSpot.Web.Entities.Requests;

public class GetIdentityQueryRequest : IRequest<GetIdentityQueryResponse?>
{
}
