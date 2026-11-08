using System;
using Xunit;
namespace ParkSpot.Tests.Support;

public sealed class MySqlFactAttribute : FactAttribute
{
    public MySqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PARKSPOT_TEST_MYSQL")))
            Skip = "Run scripts/Test-Local.ps1 for real MySQL/API tests.";
    }
}
