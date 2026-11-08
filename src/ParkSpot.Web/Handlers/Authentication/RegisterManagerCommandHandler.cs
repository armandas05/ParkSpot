using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IAuthenticationService = ParkSpot.Web.Services.Interfaces.IAuthenticationService;

namespace ParkSpot.Web.Handlers.Authentication;

public class RegisterManagerCommandHandler : IRequestHandler<RegisterManagerCommandRequest, int>
{
    private readonly IAuthenticationService _authenticationService;

    public RegisterManagerCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<int> Handle(RegisterManagerCommandRequest request, CancellationToken cancellationToken)
    {
        return await _authenticationService.RegisterManagerAsync(request, cancellationToken);
    }
}
