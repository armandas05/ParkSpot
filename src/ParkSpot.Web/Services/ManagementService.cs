using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Repositories.Interfaces;
using ParkSpot.Web.Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services;

public class ManagementService : IManagementService
{
    private readonly IParkingRepository _parkingRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IInputValidator _inputValidator;

    public ManagementService(
        IParkingRepository parkingRepository,
        TimeProvider timeProvider,
        IInputValidator inputValidator)
    {
        _parkingRepository = parkingRepository;
        _timeProvider = timeProvider;
        _inputValidator = inputValidator;
    }

    public async Task<ParkingLot> CreateLotAsync(int organizationId, CreateLotCommandRequest request, CancellationToken cancellationToken)
    {
        var parkingLot = new ParkingLot
        {
            OrganizationId = organizationId,
            Name = _inputValidator.ValidateRequiredText(request.Data.Name, "Pavadinimas", 120),
            Address = _inputValidator.ValidateRequiredText(request.Data.Address, "Adresas", 240),
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _parkingRepository.AddLotAsync(parkingLot, cancellationToken);

        return parkingLot;
    }

    public async Task AddSpotsAsync(int organizationId, AddSpotsCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.Data.Count < 1 || request.Data.Count > 500)
            throw new DomainException("Nurodykite 1–500 vietų kiekį.");

        if (!await _parkingRepository.ParkingLotExistsAsync(request.ParkingLotId, organizationId, cancellationToken))
            throw new DomainException("Aikštelė nerasta.", 404);

        var existingNumbers = await _parkingRepository.GetSpotNumbersAsync(organizationId, request.ParkingLotId, cancellationToken);

        const string prefix = "A0";

        var lastNumber = existingNumbers.Where(number => number.StartsWith(prefix))
            .Select(number => int.TryParse(number[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0).Max();

        if (lastNumber > 99999 - request.Data.Count)
            throw new DomainException("Šioje aikštelėje pasiektas vietų numeracijos limitas.");

        var spotNumbers = Enumerable.Range(lastNumber + 1, request.Data.Count).Select(number => $"{prefix}{number:D2}").ToArray();

        await _parkingRepository.AddSpotsAsync(spotNumbers.Select(n => new ParkingSpot
        {
            OrganizationId = organizationId,
            ParkingLotId = request.ParkingLotId,
            Number = n
        }), cancellationToken);
    }

    public async Task SetAvailabilityAsync(int organizationId, SetAvailabilityCommandRequest request, CancellationToken cancellationToken)
    {
        var parkingSpot = await _parkingRepository.FindSpotForOrganizationAsync(request.ParkingSpotId, organizationId, cancellationToken)
                    ?? throw new DomainException("Vieta nerasta.", 404);

        parkingSpot.IsReservable = request.Data.IsReservable;

        await _parkingRepository.UpdateAvailabilityAsync(parkingSpot, cancellationToken);
    }
}
