using System;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Database;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Web.Repositories;

public class ReservationReadRepository : IReservationReadRepository
{
    private readonly IDbRepository _repository;

    public ReservationReadRepository(IDbRepository repository)
    {
        _repository = repository;
    }

    public async Task<PageResult<ReservationRow>> GetReservationsAsync(Actor actor, int page, CancellationToken cancellationToken)
    {
        var countCommand = new RawSqlCommand("""
            SELECT COUNT(*)
            FROM Reservations r
            JOIN ParkingSpots s ON s.Id = r.ParkingSpotId AND s.OrganizationId = r.OrganizationId
            JOIN ParkingLots l ON l.Id = s.ParkingLotId AND l.OrganizationId = s.OrganizationId
            WHERE r.Status = 'Created'
              AND ((@IsManager = 1 AND r.OrganizationId = @OrganizationId)
                OR (@IsManager = 0 AND r.UserId = @UserId))
            """);

        var listCommand = new RawSqlCommand("""
            SELECT r.ReservationId, r.UserId, l.Name AS LotName, s.Number AS SpotNumber,
                   r.StartTime, r.EndTime, r.CreatedAt,
                   EXISTS(SELECT 1 FROM Reservations c
                          WHERE c.ReservationId = r.ReservationId AND c.Status = 'Cancelled') AS Cancelled
            FROM Reservations r
            JOIN ParkingSpots s ON s.Id = r.ParkingSpotId AND s.OrganizationId = r.OrganizationId
            JOIN ParkingLots l ON l.Id = s.ParkingLotId AND l.OrganizationId = s.OrganizationId
            WHERE r.Status = 'Created'
              AND ((@IsManager = 1 AND r.OrganizationId = @OrganizationId)
                OR (@IsManager = 0 AND r.UserId = @UserId))
            ORDER BY r.CreatedAt DESC, r.Id DESC LIMIT 30 OFFSET @Offset;
            """);

        AddAccessParameters(countCommand, actor);
        AddAccessParameters(listCommand, actor);
        listCommand.AddParameter("@Offset", (page - 1) * 30);

        var total = await _repository.RawSqlExecuteScalarAsync<int>(countCommand, cancellationToken);
        var items = await _repository.RawSqlFetchListAsync<ReservationRow>(listCommand, cancellationToken);

        return new PageResult<ReservationRow>
        {
            Items = items,
            Total = total,
            Page = page
        };
    }

    public Task<ReservationRow?> GetReservationAsync(Actor actor, Guid reservationId, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            SELECT r.ReservationId, r.UserId, l.Name AS LotName, s.Number AS SpotNumber,
                   r.StartTime, r.EndTime, r.CreatedAt,
                   EXISTS(SELECT 1 FROM Reservations c
                          WHERE c.ReservationId = r.ReservationId AND c.Status = 'Cancelled') AS Cancelled
            FROM Reservations r
            JOIN ParkingSpots s ON s.Id = r.ParkingSpotId AND s.OrganizationId = r.OrganizationId
            JOIN ParkingLots l ON l.Id = s.ParkingLotId AND l.OrganizationId = s.OrganizationId
            WHERE r.Status = 'Created'
              AND ((@IsManager = 1 AND r.OrganizationId = @OrganizationId)
                OR (@IsManager = 0 AND r.UserId = @UserId))
              AND r.ReservationId = @ReservationId;
            """);

        AddAccessParameters(sql, actor);
        sql.AddParameter("@ReservationId", reservationId.ToString());

        return _repository.RawSqlFetchSingleAsync<ReservationRow>(sql, cancellationToken);
    }

    private static void AddAccessParameters(
        RawSqlCommand sql,
        Actor actor)
    {
        sql.AddParameter("@IsManager", actor.IsManager);
        sql.AddParameter("@OrganizationId", actor.OrganizationId);
        sql.AddParameter("@UserId", actor.Id);
    }
}
