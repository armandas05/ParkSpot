using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace ParkSpot.Web.Database;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly SqliteConnection _connection;

    public DatabaseInitializer(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _connection.OpenAsync(cancellationToken);

        using var stream = typeof(DatabaseInitializer).Assembly.GetManifestResourceStream("ParkSpot.Database.schema.sql")
            ?? throw new InvalidOperationException("Database schema resource is missing.");
        using var reader = new StreamReader(stream);
        var schema = await reader.ReadToEndAsync(cancellationToken);

        using var transaction = _connection.BeginTransaction(deferred: false);
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = schema;

        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
