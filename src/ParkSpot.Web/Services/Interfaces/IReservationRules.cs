using System;
namespace ParkSpot.Web.Services.Interfaces;

public interface IReservationRules
{
    void ValidatePeriod(DateTime start, DateTime end, DateTime now);
}
