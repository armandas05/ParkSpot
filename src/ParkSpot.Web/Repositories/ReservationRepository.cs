using ParkSpot.Web.Database;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Web.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly IDbRepository _repository;
    private DbTransaction? _transaction;

    public ReservationRepository(IDbRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReservationTransaction> BeginTransactionAndLockSpotAsync(int parkingSpotId, CancellationToken cancellationToken)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Reservation transaction is already active.");

        var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        _transaction = transaction;

        try
        {
            var sql = new RawSqlCommand("SELECT Id FROM ParkingSpots WHERE Id = @ParkingSpotId FOR UPDATE;");

            sql.AddParameter("@ParkingSpotId", parkingSpotId);

            await _repository.RawSqlFetchListAsync<int>(sql, cancellationToken, _transaction);

            return new SqlReservationTransaction(transaction, () => _transaction = null);
        }
        catch
        {
            await transaction.DisposeAsync();

            _transaction = null;

            throw;
        }
    }

    public Task<ParkingSpot?> FindParkingSpotAsync(int parkingSpotId, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("SELECT Id, OrganizationId, ParkingLotId, Number, IsReservable FROM ParkingSpots WHERE Id = @ParkingSpotId;");

        sql.AddParameter("@ParkingSpotId", parkingSpotId);

        return _repository.RawSqlFetchSingleAsync<ParkingSpot>(sql, cancellationToken, _transaction);
    }

    public Task<Reservation?> FindCreatedReservationAsync(Guid reservationId, Actor actor, CancellationToken cancellationToken)
    {
        var command = new RawSqlCommand("""
            SELECT Id, ReservationId, OrganizationId, ParkingSpotId, UserId, StartTime,
                   EndTime, Status, CreatedAt, CancelledByUserId, CancelledByManagerId
            FROM Reservations
            WHERE ReservationId = @ReservationId AND Status = 'Created'
              AND ((@IsManager = 1 AND OrganizationId = @OrganizationId)
                OR (@IsManager = 0 AND UserId = @UserId));
            """);

        command.AddParameter("@ReservationId", reservationId.ToString());
        command.AddParameter("@IsManager", actor.IsManager);
        command.AddParameter("@OrganizationId", actor.OrganizationId);
        command.AddParameter("@UserId", actor.Id);

        return _repository.RawSqlFetchSingleAsync<Reservation>(command, cancellationToken, _transaction);
    }

    public Task<bool> IsCancelledAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        var sql = new RawSqlCommand("SELECT EXISTS(SELECT 1 FROM Reservations WHERE ReservationId = @ReservationId AND Status = 'Cancelled');");

        sql.AddParameter("@ReservationId", reservationId.ToString());

        return _repository.RawSqlExecuteScalarAsync<bool>(sql, cancellationToken, _transaction);
    }

    public Task<bool> HasOverlapAsync(ReservationOverlapFilter filter, CancellationToken cancellationToken)
    {
        var command = new RawSqlCommand("""
            SELECT EXISTS(
                SELECT 1 FROM Reservations r
                WHERE r.ParkingSpotId = @ParkingSpotId AND r.Status = 'Created'
                  AND r.StartTime < @End AND @Start < r.EndTime
                  AND NOT EXISTS (
                      SELECT 1 FROM Reservations c
                      WHERE c.ReservationId = r.ReservationId AND c.Status = 'Cancelled'));
            """);

        command.AddParameter("@ParkingSpotId", filter.ParkingSpotId);
        command.AddParameter("@Start", filter.Start);
        command.AddParameter("@End", filter.End);

        return _repository.RawSqlExecuteScalarAsync<bool>(command, cancellationToken, _transaction);
    }

    public async Task LockDriverAsync(int userId, CancellationToken cancellationToken)
    {
        if (_transaction is null)
            throw new InvalidOperationException("A reservation transaction is required.");

        var sql = new RawSqlCommand("SELECT Id FROM Users WHERE Id = @UserId FOR UPDATE;");

        sql.AddParameter("@UserId", userId);

        await _repository.RawSqlFetchListAsync<int>(sql, cancellationToken, _transaction);
    }

    public Task<bool> HasDriverOverlapInLotAsync(ReservationOverlapFilter filter, CancellationToken cancellationToken)
    {
        var command = new RawSqlCommand("""
            SELECT EXISTS(
                SELECT 1 FROM Reservations r
                JOIN ParkingSpots s ON s.Id = r.ParkingSpotId AND s.OrganizationId = r.OrganizationId
                WHERE r.UserId = @UserId AND s.ParkingLotId = @ParkingLotId
                  AND r.Status = 'Created' AND r.StartTime < @End AND @Start < r.EndTime
                  AND NOT EXISTS (
                      SELECT 1 FROM Reservations c
                      WHERE c.ReservationId = r.ReservationId AND c.Status = 'Cancelled'));
            """);

        command.AddParameter("@UserId", filter.UserId);
        command.AddParameter("@ParkingLotId", filter.ParkingLotId);
        command.AddParameter("@Start", filter.Start);
        command.AddParameter("@End", filter.End);

        return _repository.RawSqlExecuteScalarAsync<bool>(command, cancellationToken, _transaction);
    }

    public async Task AppendAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        if (_transaction is null)
            throw new InvalidOperationException("A reservation transaction is required.");

        var command = new RawSqlCommand("""
            INSERT INTO Reservations
                (ReservationId, OrganizationId, ParkingSpotId, UserId, StartTime, EndTime,
                 Status, CreatedAt, CancelledByUserId, CancelledByManagerId)
            VALUES
                (@ReservationId, @OrganizationId, @ParkingSpotId, @UserId, @StartTime, @EndTime,
                 @Status, @CreatedAt, @CancelledByUserId, @CancelledByManagerId);
            SELECT LAST_INSERT_ID();
            """);

        command.AddParameter("@ReservationId", reservation.ReservationId.ToString());
        command.AddParameter("@OrganizationId", reservation.OrganizationId);
        command.AddParameter("@ParkingSpotId", reservation.ParkingSpotId);
        command.AddParameter("@UserId", reservation.UserId);
        command.AddParameter("@StartTime", reservation.StartTime);
        command.AddParameter("@EndTime", reservation.EndTime);
        command.AddParameter("@Status", reservation.Status.ToString());
        command.AddParameter("@CreatedAt", reservation.CreatedAt);
        command.AddParameter("@CancelledByUserId", reservation.CancelledByUserId);
        command.AddParameter("@CancelledByManagerId", reservation.CancelledByManagerId);

        reservation.Id = await _repository.RawSqlFetchRequiredSingleAsync<long>(command, cancellationToken, _transaction);
    }
}
