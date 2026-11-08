using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Repositories.Interfaces;

public interface IParkingRepository
{
    Task<IReadOnlyList<ParkingLotRow>> GetLotsAsync(Actor actor, CancellationToken cancellationToken);
    Task<bool> ParkingLotExistsAsync(int parkingLotId, int? organizationId, CancellationToken cancellationToken);
    Task<PageResult<ParkingSpotRow>> GetSpotsAsync(ParkingSpotFilter filter, CancellationToken cancellationToken);
    Task AddLotAsync(ParkingLot parkingLot, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetSpotNumbersAsync(int organizationId, int parkingLotId, CancellationToken cancellationToken);
    Task AddSpotsAsync(IEnumerable<ParkingSpot> spots, CancellationToken cancellationToken);
    Task<ParkingSpot?> FindSpotForOrganizationAsync(int parkingSpotId, int organizationId, CancellationToken cancellationToken);
    Task UpdateAvailabilityAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken);
}
