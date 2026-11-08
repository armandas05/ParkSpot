using ParkSpot.Web.Repositories.Interfaces;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Tests.Support;

internal sealed class Transaction : IReservationTransaction
{
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await ValueTask.CompletedTask;
    }
}
