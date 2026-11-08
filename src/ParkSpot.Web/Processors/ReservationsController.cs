using MediatR;
using ParkSpot.Web.Entities.Requests;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ParkSpot.Web.Processors;

[ApiController]
[Authorize]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "Driver")]
    public async Task<IActionResult> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _mediator.Send(new CreateReservationCommandRequest
        {
            Data = request
        }, cancellationToken);

        return CreatedAtAction(nameof(GetReservation), new
        {
            reservationId = reservation.ReservationId
        },
            new
            {
                reservation.ReservationId,
                reservation.ParkingSpotId,
                reservation.StartTime,
                reservation.EndTime
            });
    }

    [HttpPost("now")]
    [Authorize(Policy = "Driver")]
    public async Task<IActionResult> CreateNow(ReserveNowRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _mediator.Send(new ReserveNowCommandRequest
        {
            Data = request
        }, cancellationToken);

        return CreatedAtAction(nameof(GetReservation), new
        {
            reservationId = reservation.ReservationId
        },
            new
            {
                reservation.ReservationId,
                reservation.ParkingSpotId,
                reservation.StartTime,
                reservation.EndTime
            });
    }

    [HttpGet]
    public async Task<IActionResult> GetReservations(CancellationToken cancellationToken, int page = 1)
    {
        return Ok(await _mediator.Send(new GetReservationsQueryRequest
        {
            Page = page
        }, cancellationToken));
    }

    [HttpGet("{reservationId:guid}")]
    public async Task<IActionResult> GetReservation(Guid reservationId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetReservationQueryRequest
        {
            ReservationId = reservationId
        }, cancellationToken));
    }

    [HttpPost("{reservationId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid reservationId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelReservationCommandRequest
        {
            ReservationId = reservationId
        }, cancellationToken);

        return NoContent();
    }
}
