using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<IReservationTransaction> BeginTransactionAndLockSpotAsync(int parkingSpotId, CancellationToken cancellationToken);
    Task<ParkingSpot?> FindParkingSpotAsync(int parkingSpotId, CancellationToken cancellationToken);
    Task<Reservation?> FindCreatedReservationAsync(Guid reservationId, Actor actor, CancellationToken cancellationToken);
    Task<bool> IsCancelledAsync(Guid reservationId, CancellationToken cancellationToken);
    Task<bool> HasOverlapAsync(ReservationOverlapFilter filter, CancellationToken cancellationToken);
    Task LockDriverAsync(int userId, CancellationToken cancellationToken);
    Task<bool> HasDriverOverlapInLotAsync(ReservationOverlapFilter filter, CancellationToken cancellationToken);
    Task AppendAsync(Reservation reservation, CancellationToken cancellationToken);
}
