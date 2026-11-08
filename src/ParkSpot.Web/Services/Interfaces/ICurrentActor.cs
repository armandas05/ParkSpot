using ParkSpot.Web.Models;
namespace ParkSpot.Web.Services.Interfaces;

public interface ICurrentActor
{
    Actor GetActor();
    AccountIdentity? GetIdentity();
}
