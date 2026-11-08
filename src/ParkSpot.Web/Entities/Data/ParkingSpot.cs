namespace ParkSpot.Web.Entities.Data;

public class ParkingSpot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ParkingLotId { get; set; }
    public string Number { get; set; } = "";
    public bool IsReservable { get; set; } = true;
}
