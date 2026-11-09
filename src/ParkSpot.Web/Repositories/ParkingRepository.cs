using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Database;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Web.Repositories;

public class ParkingRepository : IParkingRepository
{
    private readonly IDbRepository _repository;

    public ParkingRepository(IDbRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ParkingLotRow>> GetLotsAsync(Actor actor, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            SELECT Id, Name, Address, OrganizationId FROM ParkingLots
            WHERE @IsManager = 0 OR OrganizationId = @OrganizationId
            ORDER BY Name, Id;
            """);

        sql.AddParameter("@IsManager", actor.IsManager);
        sql.AddParameter("@OrganizationId", actor.OrganizationId);

        return _repository.RawSqlFetchListAsync<ParkingLotRow>(sql, cancellationToken);
    }

    public Task<bool> ParkingLotExistsAsync(int parkingLotId, int? organizationId, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            SELECT EXISTS(SELECT 1 FROM ParkingLots
                WHERE Id = @ParkingLotId AND (@OrganizationId IS NULL OR OrganizationId = @OrganizationId));
            """);

        sql.AddParameter("@ParkingLotId", parkingLotId);
        sql.AddParameter("@OrganizationId", organizationId);

        return _repository.RawSqlExecuteScalarAsync<bool>(sql, cancellationToken);
    }

    public async Task<PageResult<ParkingSpotRow>> GetSpotsAsync(ParkingSpotFilter filter, CancellationToken cancellationToken)
    {
        const string availabilitySql = """
            SELECT s.Id, s.Number, s.IsReservable,
                   (s.IsReservable = 1 AND NOT EXISTS(
                       SELECT 1 FROM Reservations r
                       WHERE r.ParkingSpotId = s.Id AND r.Status = 'Created'
                         AND ((@Start = @End AND r.StartTime <= @Start)
                           OR (@Start <> @End AND r.StartTime < @End))
                         AND @Start < r.EndTime
                         AND NOT EXISTS(SELECT 1 FROM Reservations c
                             WHERE c.ReservationId = r.ReservationId AND c.Status = 'Cancelled')
                   )) AS IsAvailable
            FROM ParkingSpots s
            WHERE s.ParkingLotId = @ParkingLotId
              AND (@Search = '' OR instr(s.Number, @Search) > 0)
            """;

        var filteredSql = " FROM (" + availabilitySql + ") available WHERE @OnlyAvailable = 0 OR IsAvailable = 1";
        var countCommand = GenerateSpotsCommand("SELECT COUNT(*)" + filteredSql, filter);
        var listCommand = GenerateSpotsCommand("SELECT Id, Number, IsReservable, IsAvailable" + filteredSql + " ORDER BY Number, Id LIMIT 30 OFFSET @Offset;", filter);
        var total = await _repository.RawSqlExecuteScalarAsync<int>(countCommand, cancellationToken);
        var items = await _repository.RawSqlFetchListAsync<ParkingSpotRow>(listCommand, cancellationToken);

        return new PageResult<ParkingSpotRow>
        {
            Items = items,
            Total = total,
            Page = filter.Page
        };
    }

    private static RawSqlCommand GenerateSpotsCommand(string statement, ParkingSpotFilter filter)
    {
        var sql = new RawSqlCommand(statement);

        sql.AddParameter("@ParkingLotId", filter.ParkingLotId);
        sql.AddParameter("@Start", filter.Start);
        sql.AddParameter("@End", filter.End);
        sql.AddParameter("@Search", filter.Search);
        sql.AddParameter("@OnlyAvailable", filter.OnlyAvailable);
        sql.AddParameter("@Offset", (filter.Page - 1) * 30);

        return sql;
    }

    public async Task AddLotAsync(ParkingLot parkingLot, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            INSERT INTO ParkingLots (OrganizationId, Name, Address, CreatedAt)
            VALUES (@OrganizationId, @Name, @Address, @CreatedAt);
            SELECT last_insert_rowid();
            """);

        sql.AddParameter("@OrganizationId", parkingLot.OrganizationId);
        sql.AddParameter("@Name", parkingLot.Name);
        sql.AddParameter("@Address", parkingLot.Address);
        sql.AddParameter("@CreatedAt", parkingLot.CreatedAt);

        parkingLot.Id = await _repository.RawSqlFetchRequiredSingleAsync<int>(sql, cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetSpotNumbersAsync(int organizationId, int parkingLotId, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            SELECT Number FROM ParkingSpots
            WHERE OrganizationId = @OrganizationId AND ParkingLotId = @ParkingLotId;
            """);

        sql.AddParameter("@OrganizationId", organizationId);
        sql.AddParameter("@ParkingLotId", parkingLotId);

        return _repository.RawSqlFetchListAsync<string>(sql, cancellationToken);
    }

    public async Task AddSpotsAsync(IEnumerable<ParkingSpot> spots, CancellationToken cancellationToken)
    {
        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        foreach (var spot in spots)
        {
            var sql = new RawSqlCommand("""
                INSERT INTO ParkingSpots (OrganizationId, ParkingLotId, Number, IsReservable)
                VALUES (@OrganizationId, @ParkingLotId, @Number, @IsReservable);
                """);

            sql.AddParameter("@OrganizationId", spot.OrganizationId);
            sql.AddParameter("@ParkingLotId", spot.ParkingLotId);
            sql.AddParameter("@Number", spot.Number);
            sql.AddParameter("@IsReservable", spot.IsReservable);

            await _repository.RawSqlExecuteAsync(sql, cancellationToken, transaction);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public Task<ParkingSpot?> FindSpotForOrganizationAsync(int parkingSpotId, int organizationId, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            SELECT Id, OrganizationId, ParkingLotId, Number, IsReservable FROM ParkingSpots
            WHERE Id = @ParkingSpotId AND OrganizationId = @OrganizationId;
            """);

        sql.AddParameter("@ParkingSpotId", parkingSpotId);
        sql.AddParameter("@OrganizationId", organizationId);

        return _repository.RawSqlFetchSingleAsync<ParkingSpot>(sql, cancellationToken);
    }

    public async Task UpdateAvailabilityAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("""
            UPDATE ParkingSpots SET IsReservable = @IsReservable
            WHERE Id = @Id AND OrganizationId = @OrganizationId;
            """);

        sql.AddParameter("@Id", parkingSpot.Id);
        sql.AddParameter("@OrganizationId", parkingSpot.OrganizationId);
        sql.AddParameter("@IsReservable", parkingSpot.IsReservable);

        await _repository.RawSqlExecuteAsync(sql, cancellationToken);
    }
}
