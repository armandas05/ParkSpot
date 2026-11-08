using ParkSpot.Web.Entities.Data;
using System.Threading;
using System.Threading.Tasks;
namespace ParkSpot.Web.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Manager?> FindManagerByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddOrganizationWithManagerAsync(Organization organization, Manager manager, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);
}
