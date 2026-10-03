using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Web.Core.Bundles;

/// <summary>
/// Rate limits (ASP.NET Core built-in, no package). Device pairing: 5 attempts per minute and client address (LP-106),
/// so the six-digit codes cannot be guessed. Rejected requests get 429.
/// Behind the reverse proxy the address is the proxy's until forwarded headers are configured (LP-160) – then the
/// limit applies to the whole instance, which is still fine for a family.
/// </summary>
public static class RateLimitingRegistrationExtensions
{
    public const string DevicePairingPolicy = "device-pairing";

    public const int DevicePairingPermits = 5;

    public static readonly TimeSpan DevicePairingWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddMerkwerkRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(DevicePairingPolicy, DevicePairingPartition);
        });

    /// <summary>One fixed window per client address. Public for the tests.</summary>
    public static RateLimitPartition<string> DevicePairingPartition(HttpContext context) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = DevicePairingPermits,
                Window = DevicePairingWindow,
                QueueLimit = 0,
            });
}
