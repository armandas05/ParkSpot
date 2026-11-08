using ParkSpot.Web.Database;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Web.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IDbRepository _repository;

    public AccountRepository(IDbRepository repository)
    {
        _repository = repository;
    }

    public Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var command = new RawSqlCommand("""
            SELECT Id, FirstName, LastName, Email, PasswordHash, CreatedAt
            FROM Users WHERE Email = @Email;
            """);

        command.AddParameter("@Email", email);

        return _repository.RawSqlFetchSingleAsync<User>(command, cancellationToken);
    }

    public Task<Manager?> FindManagerByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var command = new RawSqlCommand("""
            SELECT Id, OrganizationId, FirstName, LastName, Email, PasswordHash, CreatedAt
            FROM Managers WHERE Email = @Email;
            """);

        command.AddParameter("@Email", email);

        return _repository.RawSqlFetchSingleAsync<Manager>(command, cancellationToken);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        var command = new RawSqlCommand("""
            INSERT INTO Users (FirstName, LastName, Email, PasswordHash, CreatedAt)
            VALUES (@FirstName, @LastName, @Email, @PasswordHash, @CreatedAt);
            SELECT LAST_INSERT_ID();
            """);

        command.AddParameter("@FirstName", user.FirstName);
        command.AddParameter("@LastName", user.LastName);
        command.AddParameter("@Email", user.Email);
        command.AddParameter("@PasswordHash", user.PasswordHash);
        command.AddParameter("@CreatedAt", user.CreatedAt);

        user.Id = await _repository.RawSqlFetchRequiredSingleAsync<int>(command, cancellationToken);
    }

    public async Task AddOrganizationWithManagerAsync(Organization organization, Manager manager, CancellationToken cancellationToken)
    {
        await using var transaction = await _repository.BeginTransactionAsync(cancellationToken);

        var organizationCommand = new RawSqlCommand("""
            INSERT INTO Organizations (Name, CreatedAt)
            VALUES (@Name, @CreatedAt);
            SELECT LAST_INSERT_ID();
            """);

        organizationCommand.AddParameter("@Name", organization.Name);
        organizationCommand.AddParameter("@CreatedAt", organization.CreatedAt);

        organization.Id = await _repository.RawSqlFetchRequiredSingleAsync<int>(organizationCommand, cancellationToken, transaction);
        manager.OrganizationId = organization.Id;

        var managerCommand = new RawSqlCommand("""
            INSERT INTO Managers (OrganizationId, FirstName, LastName, Email, PasswordHash, CreatedAt)
            VALUES (@OrganizationId, @FirstName, @LastName, @Email, @PasswordHash, @CreatedAt);
            SELECT LAST_INSERT_ID();
            """);

        managerCommand.AddParameter("@OrganizationId", manager.OrganizationId);
        managerCommand.AddParameter("@FirstName", manager.FirstName);
        managerCommand.AddParameter("@LastName", manager.LastName);
        managerCommand.AddParameter("@Email", manager.Email);
        managerCommand.AddParameter("@PasswordHash", manager.PasswordHash);
        managerCommand.AddParameter("@CreatedAt", manager.CreatedAt);

        manager.Id = await _repository.RawSqlFetchRequiredSingleAsync<int>(managerCommand, cancellationToken, transaction);

        await transaction.CommitAsync(cancellationToken);
    }
}
