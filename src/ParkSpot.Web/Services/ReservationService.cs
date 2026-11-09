using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Entities.Enums;
using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;
using ParkSpot.Web.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationReadRepository _reservationReadRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IReservationRules _reservationRules;

    public ReservationService(
        IReservationRepository reservationRepository,
        IReservationReadRepository reservationReadRepository,
        TimeProvider timeProvider,
        IReservationRules reservationRules)
    {
        _reservationRepository = reservationRepository;
        _reservationReadRepository = reservationReadRepository;
        _timeProvider = timeProvider;
        _reservationRules = reservationRules;
    }

    public async Task<PageResult<ReservationRow>> GetReservationsAsync(Actor actor, GetReservationsQueryRequest request, CancellationToken cancellationToken)
    {
        return await _reservationReadRepository.GetReservationsAsync(actor, Math.Clamp(request.Page, 1, 100000), cancellationToken);
    }

    public async Task<ReservationRow> GetReservationAsync(Actor actor, GetReservationQueryRequest request, CancellationToken cancellationToken)
    {
        return await _reservationReadRepository.GetReservationAsync(actor, request.ReservationId, cancellationToken)
            ?? throw new DomainException("Rezervacija nerasta.", 404);
    }

    public async Task<Reservation> CreateNowAsync(Actor actor, ReserveNowCommandRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Data.DurationMinutes < 1 || request.Data.DurationMinutes > 10080)
            throw new DomainException("Rezervavimo trukmė turi būti nuo 1 minutės iki 7 parų.");

        var now = _timeProvider.GetUtcNow();

        var data = new ReservationCreationData
        {
            ParkingSpotId = request.Data.ParkingSpotId,
            Start = now.UtcDateTime,
            End = now.AddMinutes(request.Data.DurationMinutes).UtcDateTime,
            RequestedAt = now.UtcDateTime
        };

        return await CreateReservationAsync(actor, data, cancellationToken);
    }

    public async Task<Reservation> CreateAsync(Actor actor, CreateReservationCommandRequest request, CancellationToken cancellationToken = default)
    {
        var data = new ReservationCreationData
        {
            ParkingSpotId = request.Data.ParkingSpotId,
            Start = request.Data.Start.UtcDateTime,
            End = request.Data.End.UtcDateTime,
            RequestedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        return await CreateReservationAsync(actor, data, cancellationToken);
    }

    private async Task<Reservation> CreateReservationAsync(Actor actor, ReservationCreationData data, CancellationToken cancellationToken)
    {
        if (actor.IsManager)
            throw new DomainException("Rezervuoti gali vairuotojas.", 403);

        _reservationRules.ValidatePeriod(data.Start, data.End, data.RequestedAt);

        await using var transaction = await _reservationRepository.BeginReservationTransactionAsync(cancellationToken);

        var parkingSpot = await _reservationRepository.FindParkingSpotAsync(data.ParkingSpotId, cancellationToken)
            ?? throw new DomainException("Parkavimo vieta nerasta.", 404);

        if (!parkingSpot.IsReservable)
            throw new DomainException("Ši vieta uždaryta naujoms rezervacijoms.", 409);

        var filter = new ReservationOverlapFilter
        {
            ParkingSpotId = parkingSpot.Id,
            ParkingLotId = parkingSpot.ParkingLotId,
            UserId = actor.Id,
            Start = data.Start,
            End = data.End
        };

        if (await _reservationRepository.HasDriverOverlapInLotAsync(filter, cancellationToken))
            throw new DomainException("Šioje aikštelėje pasirinktu laikotarpiu jau turite rezervuotą vietą. Pirmiausia atšaukite ankstesnę rezervaciją arba pasirinkite kitą laiką.", 409);

        if (await _reservationRepository.HasOverlapAsync(filter, cancellationToken))
            throw new DomainException("Pasirinkta trukmė persidengia su kita rezervacija. Pasirinkite trumpesnę trukmę arba kitą vietą.", 409);

        var reservation = new Reservation
        {
            ReservationId = Guid.NewGuid(),
            OrganizationId = parkingSpot.OrganizationId,
            ParkingSpotId = parkingSpot.Id,
            UserId = actor.Id,
            StartTime = data.Start,
            EndTime = data.End,
            Status = ReservationStatus.Created,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _reservationRepository.AppendAsync(reservation, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return reservation;
    }

    public async Task CancelAsync(Actor actor, CancelReservationCommandRequest request, CancellationToken cancellationToken = default)
    {
        var createdReservation = await _reservationRepository.FindCreatedReservationAsync(request.ReservationId, actor, cancellationToken)
            ?? throw new DomainException("Rezervacija nerasta.", 404);

        await using var transaction = await _reservationRepository.BeginReservationTransactionAsync(cancellationToken);

        if (createdReservation.EndTime <= _timeProvider.GetUtcNow().UtcDateTime)
            throw new DomainException("Pasibaigusios rezervacijos atšaukti negalima.", 409);

        if (await _reservationRepository.IsCancelledAsync(request.ReservationId, cancellationToken))
            throw new DomainException("Rezervacija jau atšaukta.", 409);

        await _reservationRepository.AppendAsync(new Reservation
        {
            ReservationId = request.ReservationId,
            OrganizationId = createdReservation.OrganizationId,
            ParkingSpotId = createdReservation.ParkingSpotId,
            UserId = createdReservation.UserId,
            StartTime = createdReservation.StartTime,
            EndTime = createdReservation.EndTime,
            Status = ReservationStatus.Cancelled,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            CancelledByUserId = actor.IsManager ? null : actor.Id,
            CancelledByManagerId = actor.IsManager ? actor.Id : null
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
