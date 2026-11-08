using ParkSpot.Web.Entities.Requests;
using MediatR;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ParkSpot.Web.Processors;

[ApiController]
[Authorize]
[Route("api/lots")]
public class ParkingLotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ParkingLotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetLots(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetLotsQueryRequest(), cancellationToken));
    }

    [HttpGet("{parkingLotId:int}/spots")]
    public async Task<IActionResult> GetSpots(int parkingLotId, DateTimeOffset? start, DateTimeOffset? end, CancellationToken cancellationToken, int page = 1, string? search = null, bool onlyAvailable = false)
    {
        return Ok(await _mediator.Send(new GetSpotsQueryRequest
        {
            ParkingLotId = parkingLotId,
            Start = start,
            End = end,
            Page = page,
            Search = search,
            OnlyAvailable = onlyAvailable
        }, cancellationToken));
    }
}
