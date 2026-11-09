using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Entities.Enums;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Tests.Support;

internal sealed class InMemoryReservationRepository : IReservationRepository
{
    public ParkingSpot Spot { get; } = new()
    {
        Id = 1,
        OrganizationId = 1,
        ParkingLotId = 1,
        Number = "A001"
    };
    public List<Reservation> Events { get; } = [];

    public async Task<IReservationTransaction> BeginReservationTransactionAsync(CancellationToken cancellationToken)
    {
        return await Task.FromResult<IReservationTransaction>(new Transaction());
    }

    public async Task<ParkingSpot?> FindParkingSpotAsync(int id, CancellationToken cancellationToken)
    {
        return await Task.FromResult<ParkingSpot?>(id == Spot.Id ? Spot : null);
    }

    public async Task<Reservation?> FindCreatedReservationAsync(Guid id, Actor actor, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Events.SingleOrDefault(r => r.ReservationId == id && r.Status == ReservationStatus.Created && (actor.IsManager ? r.OrganizationId == actor.OrganizationId : r.UserId == actor.Id)));
    }

    public async Task<bool> IsCancelledAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Events.Any(r => r.ReservationId == id && r.Status == ReservationStatus.Cancelled));
    }

    public async Task<bool> HasOverlapAsync(ReservationOverlapFilter filter, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Events.Any(r => r.ParkingSpotId == filter.ParkingSpotId && r.Status == ReservationStatus.Created && r.StartTime < filter.End && filter.Start < r.EndTime && !Events.Any(c => c.ReservationId == r.ReservationId && c.Status == ReservationStatus.Cancelled)));
    }

    public async Task<bool> HasDriverOverlapInLotAsync(ReservationOverlapFilter filter, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Events.Any(reservation => reservation.UserId == filter.UserId && reservation.ParkingSpotId == Spot.Id && Spot.ParkingLotId == filter.ParkingLotId && reservation.Status == ReservationStatus.Created && reservation.StartTime < filter.End && filter.Start < reservation.EndTime && !Events.Any(cancelled => cancelled.ReservationId == reservation.ReservationId && cancelled.Status == ReservationStatus.Cancelled)));
    }

    public Task AppendAsync(Reservation r, CancellationToken cancellationToken)
    {
        Events.Add(r);

        return Task.CompletedTask;
    }
}
