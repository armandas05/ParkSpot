using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Models;
using ParkSpot.Web.Repositories.Interfaces;
using ParkSpot.Web.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
namespace ParkSpot.Web.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IInputValidator _inputValidator;
    private readonly IPasswordHasher<User> _userPasswordHasher;
    private readonly IPasswordHasher<Manager> _managerPasswordHasher;
    private readonly TimeProvider _timeProvider;

    public AuthenticationService(
        IAccountRepository accountRepository,
        IInputValidator inputValidator,
        IPasswordHasher<User> userPasswordHasher,
        IPasswordHasher<Manager> managerPasswordHasher,
        TimeProvider timeProvider)
    {
        _accountRepository = accountRepository;
        _inputValidator = inputValidator;
        _userPasswordHasher = userPasswordHasher;
        _managerPasswordHasher = managerPasswordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<int> RegisterUserAsync(RegisterCommandRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Data.Email);

        ValidatePassword(request.Data.Password);

        if (await _accountRepository.FindUserByEmailAsync(email, cancellationToken) is not null)
            throw new DomainException("Šis el. paštas jau užregistruotas.", 409);

        var user = new User
        {
            FirstName = _inputValidator.ValidateRequiredText(request.Data.FirstName, "Vardas", 80),
            LastName = _inputValidator.ValidateRequiredText(request.Data.LastName, "Pavardė", 80),
            Email = email,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        user.PasswordHash = _userPasswordHasher.HashPassword(user, request.Data.Password);

        await _accountRepository.AddUserAsync(user, cancellationToken);

        return user.Id;
    }

    public async Task<int> RegisterManagerAsync(RegisterManagerCommandRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Data.Email);

        ValidatePassword(request.Data.Password);

        if (await _accountRepository.FindManagerByEmailAsync(email, cancellationToken) is not null)
            throw new DomainException("Šis administratoriaus el. paštas jau užregistruotas.", 409);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var organization = new Organization
        {
            Name = _inputValidator.ValidateRequiredText(request.Data.OrganizationName, "Organizacijos pavadinimas", 120),
            CreatedAt = now
        };
        var manager = new Manager
        {
            FirstName = _inputValidator.ValidateRequiredText(request.Data.FirstName, "Vardas", 80),
            LastName = _inputValidator.ValidateRequiredText(request.Data.LastName, "Pavardė", 80),
            Email = email,
            CreatedAt = now
        };

        manager.PasswordHash = _managerPasswordHasher.HashPassword(manager, request.Data.Password);

        await _accountRepository.AddOrganizationWithManagerAsync(organization, manager, cancellationToken);

        return manager.Id;
    }

    public async Task<AccountIdentity> LoginAsync(LoginCommandRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Data.Email);

        if (string.IsNullOrEmpty(request.Data.Password) || request.Data.Password.Length > 128)
            throw new DomainException("Neteisingas el. paštas arba slaptažodis.", 401);

        if (request.AccountType == "driver")
        {
            var user = await _accountRepository.FindUserByEmailAsync(email, cancellationToken);

            if (user is null || _userPasswordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Data.Password) == PasswordVerificationResult.Failed)
                throw new DomainException("Neteisingas el. paštas arba slaptažodis.", 401);

            return new AccountIdentity
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Role = "Driver",
                OrganizationId = null
            };
        }

        if (request.AccountType == "manager")
        {
            var manager = await _accountRepository.FindManagerByEmailAsync(email, cancellationToken);

            if (manager is null || _managerPasswordHasher.VerifyHashedPassword(manager, manager.PasswordHash, request.Data.Password) == PasswordVerificationResult.Failed)
                throw new DomainException("Neteisingas el. paštas arba slaptažodis.", 401);

            return new AccountIdentity
            {
                Id = manager.Id,
                Name = $"{manager.FirstName} {manager.LastName}",
                Role = "Manager",
                OrganizationId = manager.OrganizationId
            };
        }

        throw new DomainException("Prisijungimo puslapis nerastas.", 404);
    }

    private static void ValidatePassword(string? password)
    {
        if (password is null || password.Length < 12 || password.Length > 128)
            throw new DomainException("Slaptažodis turi būti 12–128 simbolių.");
    }

    private string NormalizeEmail(string? value)
    {
        var email = _inputValidator.ValidateRequiredText(value, "El. paštas", 254).ToLowerInvariant();

        if (!MailAddress.TryCreate(email, out var address) || address.Address != email)
            throw new DomainException("Įveskite tinkamą el. pašto adresą.");

        return email;
    }
}
