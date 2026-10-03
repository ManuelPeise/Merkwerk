using System.Net;
using Microsoft.AspNetCore.Http;
using Web.Core.Bundles;

namespace Logic.Devices.Tests;

/// <summary>LP-106: pairing is limited to 5 attempts per minute and client address (429 after that).</summary>
public sealed class PairingRateLimitTests
{
    [Fact]
    public void DevicePairingPartition_SixthAttemptWithinAMinute_IsRejected()
    {
        // Arrange
        var partition = RateLimitingRegistrationExtensions.DevicePairingPartition(ContextFrom("192.0.2.10"));
        using var limiter = partition.Factory(partition.PartitionKey);

        // Act
        var permitted = Enumerable.Range(0, RateLimitingRegistrationExtensions.DevicePairingPermits)
            .Select(_ => limiter.AttemptAcquire().IsAcquired)
            .ToList();
        var sixth = limiter.AttemptAcquire();

        // Assert
        Assert.DoesNotContain(false, permitted);
        Assert.False(sixth.IsAcquired);
    }

    [Fact]
    public void DevicePairingPartition_OtherAddress_HasItsOwnLimit()
    {
        var first = RateLimitingRegistrationExtensions.DevicePairingPartition(ContextFrom("192.0.2.10"));
        var second = RateLimitingRegistrationExtensions.DevicePairingPartition(ContextFrom("192.0.2.11"));

        Assert.NotEqual(first.PartitionKey, second.PartitionKey);
    }

    private static DefaultHttpContext ContextFrom(string address)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        return context;
    }
}
