using System;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Identity;
using ParkSpot.Web.Entities.Data;

namespace ParkSpot.Tests.Support;

internal static class TestDatabaseData
{
    public static async Task CreateAsync(MySqlConnection connection)
    {
        const string password = "ParkSpot-Test-2026!";
        var userPasswordHasher = new PasswordHasher<User>();
        var managerPasswordHasher = new PasswordHasher<Manager>();
        var now = DateTime.UtcNow;

        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        for (var organizationId = 1; organizationId <= 2; organizationId++)
        {
            await connection.ExecuteAsync("INSERT INTO Organizations (Id, Name, CreatedAt) VALUES (@Id, @Name, @CreatedAt);",
            new
            {
                Id = organizationId,
                Name = $"Organizacija {organizationId}",
                CreatedAt = now
            }, transaction);

            var user = new User
            {
                Id = organizationId,
                FirstName = "Vairuotojas",
                LastName = "Test",
                Email = $"driver{organizationId}@parkspot.test",
                CreatedAt = now
            };

            user.PasswordHash = userPasswordHasher.HashPassword(user, password);

            await connection.ExecuteAsync("""
                INSERT INTO Users (Id, FirstName, LastName, Email, PasswordHash, CreatedAt)
                VALUES (@Id, @FirstName, @LastName, @Email, @PasswordHash, @CreatedAt);
                """, user, transaction);

            var manager = new Manager
            {
                Id = organizationId,
                OrganizationId = organizationId,
                FirstName = "Administratorius",
                LastName = "Test",
                Email = $"manager{organizationId}@parkspot.test",
                CreatedAt = now
            };

            manager.PasswordHash = managerPasswordHasher.HashPassword(manager, password);

            await connection.ExecuteAsync("""
                INSERT INTO Managers (Id, OrganizationId, FirstName, LastName, Email, PasswordHash, CreatedAt)
                VALUES (@Id, @OrganizationId, @FirstName, @LastName, @Email, @PasswordHash, @CreatedAt);
                """, manager, transaction);
            await connection.ExecuteAsync("""
                INSERT INTO ParkingLots (Id, OrganizationId, Name, Address, CreatedAt)
                VALUES (@Id, @OrganizationId, @Name, @Address, @CreatedAt);
                """, new
            {
                Id = organizationId,
                OrganizationId = organizationId,
                Name = $"Aikštelė {organizationId}",
                Address = "Vilnius",
                CreatedAt = now
            }, transaction);

            for (var number = 1; number <= 6; number++)
            {
                await connection.ExecuteAsync("""
                    INSERT INTO ParkingSpots (Id, OrganizationId, ParkingLotId, Number, IsReservable)
                    VALUES (@Id, @OrganizationId, @ParkingLotId, @Number, @IsReservable);
                    """, new
                {
                    Id = (organizationId - 1) * 6 + number,
                    OrganizationId = organizationId,
                    ParkingLotId = organizationId,
                    Number = $"A{number:D3}",
                    IsReservable = number != 6
                }, transaction);
            }

            await connection.ExecuteAsync("""
                INSERT INTO Reservations
                    (ReservationId, OrganizationId, ParkingSpotId, UserId, StartTime, EndTime, Status, CreatedAt)
                VALUES (@ReservationId, @OrganizationId, @ParkingSpotId, 1, @StartTime, @EndTime, 'Created', @CreatedAt);
                """, new
            {
                ReservationId = Guid.NewGuid().ToString(),
                OrganizationId = organizationId,
                ParkingSpotId = (organizationId - 1) * 6 + 1,
                StartTime = now.Date.AddDays(1).AddHours(8),
                EndTime = now.Date.AddDays(1).AddHours(10),
                CreatedAt = now
            }, transaction);
        }

        await transaction.CommitAsync();
    }
}
