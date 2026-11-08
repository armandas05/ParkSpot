using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;
using ParkSpot.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services;

public class ParkingService : IParkingService
{
    private readonly IParkingRepository _parkingRepository;
    private readonly IReservationRules _reservationRules;
    private readonly TimeProvider _timeProvider;

    public ParkingService(
        IParkingRepository parkingRepository,
        IReservationRules reservationRules,
        TimeProvider timeProvider)
    {
        _parkingRepository = parkingRepository;
        _reservationRules = reservationRules;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ParkingLotRow>> GetLotsAsync(Actor actor, CancellationToken cancellationToken)
    {
        return await _parkingRepository.GetLotsAsync(actor, cancellationToken);
    }
    public async Task<PageResult<ParkingSpotRow>> GetSpotsAsync(Actor actor, GetSpotsQueryRequest request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        if (request.Start.HasValue != request.End.HasValue)
            throw new DomainException("Nurodykite laikotarpio pradžią ir pabaigą.");

        if (request.Start.HasValue)
            _reservationRules.ValidatePeriod(request.Start.Value.UtcDateTime, request.End!.Value.UtcDateTime, now);

        if (!await _parkingRepository.ParkingLotExistsAsync(request.ParkingLotId, actor.OrganizationId, cancellationToken))
            throw new DomainException("Aikštelė nerasta.", 404);

        var filter = new ParkingSpotFilter
        {
            ParkingLotId = request.ParkingLotId,
            Start = request.Start?.UtcDateTime ?? now,
            End = request.End?.UtcDateTime ?? now,
            Page = Math.Clamp(request.Page, 1, 100000),
            Search = request.Search?.Trim() ?? "",
            OnlyAvailable = request.OnlyAvailable
        };

        return await _parkingRepository.GetSpotsAsync(filter, cancellationToken);
    }
}
