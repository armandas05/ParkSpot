using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Database;
using Dapper;
using MySql.Data.MySqlClient;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Web.Repositories;

public class DbRepository : IDbRepository
{
    private readonly MySqlConnection _connection;

    public DbRepository(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(cancellationToken);

        return await _connection.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public Task<int> RawSqlExecuteAsync(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        return _connection.ExecuteAsync(CreateCommand(sql, cancellationToken, transaction));
    }

    public async Task<IReadOnlyList<T>> RawSqlFetchListAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var rows = await _connection.QueryAsync<T>(CreateCommand(sql, cancellationToken, transaction));

        return rows.AsList();
    }

    public Task<T> RawSqlFetchRequiredSingleAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        return _connection.QuerySingleAsync<T>(CreateCommand(sql, cancellationToken, transaction));
    }

    public Task<T?> RawSqlFetchSingleAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        return _connection.QuerySingleOrDefaultAsync<T>(CreateCommand(sql, cancellationToken, transaction));
    }

    public Task<T?> RawSqlExecuteScalarAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        return _connection.ExecuteScalarAsync<T>(CreateCommand(sql, cancellationToken, transaction));
    }

    private static CommandDefinition CreateCommand(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        var parameters = new DynamicParameters();

        foreach (var parameter in sql.Parameters)
            parameters.Add(parameter.Key, parameter.Value);

        return new CommandDefinition(sql.CommandText, parameters, transaction,
            cancellationToken: cancellationToken);
    }
}
