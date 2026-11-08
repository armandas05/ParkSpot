using System;
namespace ParkSpot.Web.Models;

public class ReservationRow
{
    public Guid ReservationId { get; set; }
    public int UserId { get; set; }
    public required string LotName { get; set; }
    public required string SpotNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Cancelled { get; set; }
}
