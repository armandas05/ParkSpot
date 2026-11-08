using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services.Interfaces;

public interface IParkingService
{
    Task<IReadOnlyList<ParkingLotRow>> GetLotsAsync(Actor actor, CancellationToken cancellationToken);
    Task<PageResult<ParkingSpotRow>> GetSpotsAsync(Actor actor, GetSpotsQueryRequest request, CancellationToken cancellationToken);
}
