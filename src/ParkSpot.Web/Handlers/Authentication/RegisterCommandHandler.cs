using ParkSpot.Web.Entities.Requests;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IAuthenticationService = ParkSpot.Web.Services.Interfaces.IAuthenticationService;

namespace ParkSpot.Web.Handlers.Authentication;

public class RegisterCommandHandler : IRequestHandler<RegisterCommandRequest, int>
{
    private readonly IAuthenticationService _authenticationService;

    public RegisterCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<int> Handle(RegisterCommandRequest request, CancellationToken cancellationToken)
    {
        return _authenticationService.RegisterUserAsync(request, cancellationToken);
    }
}
