using ParkSpot.Web.Entities.Requests;
using ParkSpot.Web.Entities.Enums;
using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Models;
using ParkSpot.Web.Services;
using ParkSpot.Tests.Support;
using System;
using System.Threading.Tasks;
using Xunit;
namespace ParkSpot.Tests;

public class ReservationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 8, 0, 0, TimeSpan.Zero);
    private readonly InMemoryReservationRepository _reservationRepository = new();
    private ReservationService Service
    {
        get
        {
            return new(_reservationRepository, new UnusedReservationReadRepository(), new FixedTimeProvider(Now), new ReservationRules());
        }
    }

    [Fact]
    public async Task Free_spot_creates_exact_requested_reservation()
    {
        var reservation = await Service.CreateNowAsync(new Actor
        {
            Id = 1,
            OrganizationId = null
        }, new ReserveNowCommandRequest
        {
            Data = new ReserveNowRequest
            {
                ParkingSpotId = 1,
                DurationMinutes = 120
            }
        });

        Assert.Equal(1, reservation.ParkingSpotId);
        Assert.Equal(1, reservation.OrganizationId);
        Assert.Equal(Now.UtcDateTime, reservation.StartTime);
        Assert.Equal(Now.AddHours(2).UtcDateTime, reservation.EndTime);
        Assert.Equal(ReservationStatus.Created, reservation.Status);

        Assert.Single(_reservationRepository.Events);
    }

    [Fact]
    public async Task Overlapping_reservation_is_rejected_without_another_row()
    {
        await Service.CreateAsync(new Actor
        {
            Id = 1,
            OrganizationId = null
        }, new CreateReservationCommandRequest
        {
            Data = new CreateReservationRequest
            {
                ParkingSpotId = 1,
                Start = Now.AddHours(2),
                End = Now.AddHours(4)
            }
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            Service.CreateNowAsync(new Actor
            {
                Id = 2,
                OrganizationId = null
            }, new ReserveNowCommandRequest
            {
                Data = new ReserveNowRequest
                {
                    ParkingSpotId = 1,
                    DurationMinutes = 180
                }
            }));

        Assert.Equal(409, exception.StatusCode);

        Assert.Single(_reservationRepository.Events);
    }

    [Fact]
    public async Task Invalid_duration_is_rejected()
    {
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
                    Service.CreateNowAsync(new Actor
                    {
                        Id = 1,
                        OrganizationId = null
                    }, new ReserveNowCommandRequest
                    {
                        Data = new ReserveNowRequest
                        {
                            ParkingSpotId = 1,
                            DurationMinutes = 0
                        }
                    }));

        Assert.Equal(400, exception.StatusCode);

        Assert.Empty(_reservationRepository.Events);
    }
}
