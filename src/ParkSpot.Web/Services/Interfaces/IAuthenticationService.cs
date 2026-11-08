using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Models;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Services.Interfaces;

public interface IAuthenticationService
{
    Task<int> RegisterUserAsync(RegisterCommandRequest request, CancellationToken cancellationToken);
    Task<int> RegisterManagerAsync(RegisterManagerCommandRequest request, CancellationToken cancellationToken);
    Task<AccountIdentity> LoginAsync(LoginCommandRequest request, CancellationToken cancellationToken);
}
