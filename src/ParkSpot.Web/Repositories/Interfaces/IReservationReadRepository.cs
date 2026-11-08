using ParkSpot.Web.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Repositories.Interfaces;

public interface IReservationReadRepository
{
    Task<PageResult<ReservationRow>> GetReservationsAsync(Actor actor, int page, CancellationToken cancellationToken);
    Task<ReservationRow?> GetReservationAsync(Actor actor, Guid reservationId, CancellationToken cancellationToken);
}
