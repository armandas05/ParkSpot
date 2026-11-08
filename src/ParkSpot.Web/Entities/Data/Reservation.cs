using ParkSpot.Web.Entities.Enums;
using System;
namespace ParkSpot.Web.Entities.Data;

public class Reservation
{
    public long Id { get; set; }
    public Guid ReservationId { get; set; }
    public int OrganizationId { get; set; }
    public int ParkingSpotId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public int? CancelledByManagerId { get; set; }
}
