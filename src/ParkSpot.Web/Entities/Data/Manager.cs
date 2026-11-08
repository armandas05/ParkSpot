using System;
namespace ParkSpot.Web.Entities.Data;

public class Manager
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
