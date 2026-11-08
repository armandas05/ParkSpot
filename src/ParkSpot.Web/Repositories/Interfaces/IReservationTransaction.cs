using System;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Repositories.Interfaces;

public interface IReservationTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
