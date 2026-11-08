using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Entities.Data;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services.Interfaces;

public interface IManagementService
{
    Task<ParkingLot> CreateLotAsync(int organizationId, CreateLotCommandRequest request, CancellationToken cancellationToken);
    Task AddSpotsAsync(int organizationId, AddSpotsCommandRequest request, CancellationToken cancellationToken);
    Task SetAvailabilityAsync(int organizationId, SetAvailabilityCommandRequest request, CancellationToken cancellationToken);
}
