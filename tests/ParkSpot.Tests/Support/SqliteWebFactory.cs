using System.IO;
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
using Microsoft.Data.Sqlite;

namespace ParkSpot.Tests.Support;

public class SqliteWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), "parkspot_test_" + Guid.NewGuid().ToString("N") + ".db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<SqliteConnection>();
            var settings = new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                ForeignKeys = true,
                Pooling = false,
                DefaultTimeout = 30
            };

            services.AddScoped<SqliteConnection>(_ => new SqliteConnection(settings.ToString()));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var connection = scope.ServiceProvider.GetRequiredService<SqliteConnection>();

        await TestDatabaseData.CreateAsync(connection);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        File.Delete(_databasePath);
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
