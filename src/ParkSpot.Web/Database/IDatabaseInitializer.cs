using System.Threading;
using System.Threading.Tasks;

namespace ParkSpot.Web.Database;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
