using System;
namespace ParkSpot.Tests.Support;

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _fixedUtcTime;

    public FixedTimeProvider(DateTimeOffset fixedUtcTime)
    {
        _fixedUtcTime = fixedUtcTime;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _fixedUtcTime;
    }
}
