using Dapper;
using ParkSpot.Web.Database;
using Microsoft.Data.Sqlite;
using ParkSpot.Tests.Support;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace ParkSpot.Tests;

public class ApiTests : IClassFixture<SqliteWebFactory>
{
    private readonly SqliteWebFactory _sqliteWebFactory;

    public ApiTests(SqliteWebFactory sqliteWebFactory)
    {
        _sqliteWebFactory = sqliteWebFactory;
    }

    private static object Request(int spot, int day)
    {
        return new
        {
            parkingSpotId = spot,
            start = DateTimeOffset.UtcNow.Date.AddDays(day).AddHours(12).ToString("O") + "Z",
            end = DateTimeOffset.UtcNow.Date.AddDays(day).AddHours(14).ToString("O") + "Z"
        };
    }

    [Fact]
    public async Task Organization_isolation_blocks_read_cancel_and_spot_changes()
    {
        using var registrationClient = _sqliteWebFactory.CreateClient();
        var registration = new
        {
            organizationName = "Nauja organizacija",
            firstName = "Naujas",
            lastName = "Administratorius",
            email = "manager3@parkspot.test",
            password = "ParkSpot-Test-2026!"
        };

        Assert.Equal(HttpStatusCode.Created, (await registrationClient.PostAsJsonAsync("/api/auth/register/manager", registration)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await registrationClient.PostAsJsonAsync("/api/auth/register/manager", registration)).StatusCode);

        using var owner = await _sqliteWebFactory.Login("manager", 3);
        var identity = await owner.GetFromJsonAsync<JsonElement>("/api/auth/me");

        Assert.Equal("Manager", identity.GetProperty("role").GetString());
        Assert.NotEqual(1, identity.GetProperty("organizationId").GetInt32());
        Assert.Empty((await owner.GetFromJsonAsync<JsonElement>("/api/lots")).EnumerateArray());

        var lotCreation = await owner.PostAsJsonAsync("/api/manager/lots", new
        {
            name = "Nauja aikštelė",
            address = "Vilnius"
        });

        Assert.Equal(HttpStatusCode.OK, lotCreation.StatusCode);

        var lotId = (await lotCreation.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.NoContent, (await owner.PostAsJsonAsync($"/api/manager/lots/{lotId}/spots", new
        {
            count = 1
        })).StatusCode);

        var spots = await owner.GetFromJsonAsync<JsonElement>($"/api/lots/{lotId}/spots");
        var spotId = spots.GetProperty("items")[0].GetProperty("id").GetInt32();

        using var driver = await _sqliteWebFactory.Login("driver", 1);
        var creation = await driver.PostAsJsonAsync("/api/reservations", Request(spotId, 10));

        Assert.Equal(HttpStatusCode.Created, creation.StatusCode);

        var id = (await creation.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("reservationId").GetString();
        using var manager = await _sqliteWebFactory.Login("manager", 1);

        Assert.Equal(HttpStatusCode.NotFound, (await manager.GetAsync($"/api/reservations/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await manager.PostAsync($"/api/reservations/{id}/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await manager.PatchAsJsonAsync($"/api/manager/spots/{spotId}", new
        {
            isReservable = false
        })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await manager.PostAsJsonAsync($"/api/manager/lots/{lotId}/spots", new
        {
            count = 2
        })).StatusCode);

        var lots = await manager.GetFromJsonAsync<JsonElement>("/api/lots");

        Assert.NotEmpty(lots.EnumerateArray());

        Assert.All(lots.EnumerateArray(), lot => Assert.Equal(1, lot.GetProperty("organizationId").GetInt32()));

        var visible = await owner.GetFromJsonAsync<JsonElement>($"/api/reservations/{id}");

        Assert.False(visible.GetProperty("cancelled").GetBoolean());

        using var secondDriver = await _sqliteWebFactory.Login("driver", 2);
        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstAttempt = Task.Run(async () =>
        {
            await start.Task;
            return await driver.PostAsJsonAsync("/api/reservations", Request(spotId, 12));
        });
        var secondAttempt = Task.Run(async () =>
        {
            await start.Task;
            return await secondDriver.PostAsJsonAsync("/api/reservations", Request(spotId, 12));
        });

        start.SetResult(true);
        var attempts = await Task.WhenAll(firstAttempt, secondAttempt);

        Assert.Single(attempts, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(attempts, response => response.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(HttpStatusCode.NoContent, (await owner.PostAsync($"/api/reservations/{id}/cancel", null)).StatusCode);
        var cancelled = await owner.GetFromJsonAsync<JsonElement>($"/api/reservations/{id}");

        Assert.True(cancelled.GetProperty("cancelled").GetBoolean());

        using var scope = _sqliteWebFactory.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
        var connection = scope.ServiceProvider.GetRequiredService<SqliteConnection>();

        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("PRAGMA foreign_keys;"));
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Reservations WHERE ReservationId = @Id;", new { Id = id }));
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Reservations WHERE ParkingSpotId = @Id AND Status = 'Created';", new { Id = spotId }));


        Assert.True(await scope.ServiceProvider.GetRequiredService<SqliteConnection>()
            .ExecuteScalarAsync<bool>("SELECT IsReservable FROM ParkingSpots WHERE Id = @Id;", new
            {
                Id = spotId
            }));
    }
}
