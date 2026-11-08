using MediatR;
using ParkSpot.Web.Entities.Requests;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ParkSpot.Web.Processors;

[ApiController]
[Authorize(Policy = "Manager")]
[Route("api/manager")]
public class ManagementController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("lots")]
    public async Task<IActionResult> CreateLot(CreateLotRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new CreateLotCommandRequest
        {
            Data = request
        }, cancellationToken));
    }

    [HttpPost("lots/{parkingLotId:int}/spots")]
    public async Task<IActionResult> AddSpots(int parkingLotId, AddSpotsRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AddSpotsCommandRequest
        {
            ParkingLotId = parkingLotId,
            Data = request
        }, cancellationToken);

        return NoContent();
    }

    [HttpPatch("spots/{parkingSpotId:int}")]
    public async Task<IActionResult> SetAvailability(int parkingSpotId, AvailabilityRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetAvailabilityCommandRequest
        {
            ParkingSpotId = parkingSpotId,
            Data = request
        }, cancellationToken);

        return NoContent();
    }
}
