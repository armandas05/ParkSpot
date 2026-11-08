using System.IO;
using Dapper;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.Data.MySqlClient;

namespace ParkSpot.Tests.Support;

public class MySqlFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string? server = Environment.GetEnvironmentVariable("PARKSPOT_TEST_MYSQL");
    private readonly string databaseName = "parkspot_test_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<MySqlConnection>();

            var connection = new MySqlConnectionStringBuilder(server!)
            {
                Database = databaseName
            };

            services.AddScoped<MySqlConnection>(_ => new MySqlConnection(connection.ConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        if (server is null)
            return;

        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<MySqlConnection>();

        await using (var connection = new MySqlConnection(server))
        {
            await connection.OpenAsync();

            await using var createDatabase = connection.CreateCommand();

            createDatabase.CommandText = $"CREATE DATABASE `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";

            await createDatabase.ExecuteNonQueryAsync();
            await connection.ChangeDatabaseAsync(databaseName);

            await using var createSchema = connection.CreateCommand();

            createSchema.CommandText = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Database", "schema.sql"));

            await createSchema.ExecuteNonQueryAsync();
        }

        await TestDatabaseData.CreateAsync(database);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (server is not null)
        {
            await using var connection = new MySqlConnection(server);

            await connection.ExecuteAsync($"DROP DATABASE IF EXISTS `{databaseName}`;");
        }

        await DisposeAsync();
    }

    public async Task<HttpClient> Login(string kind, int id)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var result = await client.PostAsJsonAsync($"/api/auth/login/{kind}", new
        {
            email = $"{kind}{id}@parkspot.test",
            password = "ParkSpot-Test-2026!"
        });

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        return client;
    }
}
