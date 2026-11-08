using ParkSpot.Web.Exceptions;
using ParkSpot.Web.Services.Interfaces;
using System;
namespace ParkSpot.Web.Services;

public class ReservationRules : IReservationRules
{
    public void ValidatePeriod(DateTime start, DateTime end, DateTime now)
    {
        if (end <= start)
            throw new DomainException("Rezervacijos pabaiga turi būti vėlesnė už pradžią.");

        if (start < now)
            throw new DomainException("Rezervuoti praeityje negalima.");
    }
}
