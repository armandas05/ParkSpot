using System;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Tests.Support;

internal sealed class UnusedReservationReadRepository : IReservationReadRepository
{
    public Task<PageResult<ReservationRow>> GetReservationsAsync(Actor actor, int page, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Reservation reads are not expected in creation tests.");
    }

    public Task<ReservationRow?> GetReservationAsync(Actor actor, Guid reservationId, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Reservation reads are not expected in creation tests.");
    }
}
