using System;
namespace ParkSpot.Web.Entities.Data;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
