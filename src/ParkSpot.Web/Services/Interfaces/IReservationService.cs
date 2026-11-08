using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Models;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services.Interfaces;

public interface IReservationService
{
    Task<PageResult<ReservationRow>> GetReservationsAsync(Actor actor, GetReservationsQueryRequest request, CancellationToken cancellationToken);
    Task<ReservationRow> GetReservationAsync(Actor actor, GetReservationQueryRequest request, CancellationToken cancellationToken);

    Task<Reservation> CreateAsync(Actor actor, CreateReservationCommandRequest request, CancellationToken cancellationToken = default);
    Task<Reservation> CreateNowAsync(Actor actor, ReserveNowCommandRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(Actor actor, CancelReservationCommandRequest request, CancellationToken cancellationToken = default);
}
