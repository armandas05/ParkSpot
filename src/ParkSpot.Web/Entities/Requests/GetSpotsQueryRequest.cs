using System;
using MediatR;
using ParkSpot.Web.Models;

namespace ParkSpot.Web.Entities.Requests;

public class GetSpotsQueryRequest : IRequest<PageResult<ParkingSpotRow>>
{
    public int ParkingLotId { get; set; }
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public int Page { get; set; }
    public string? Search { get; set; }
    public bool OnlyAvailable { get; set; }
}
