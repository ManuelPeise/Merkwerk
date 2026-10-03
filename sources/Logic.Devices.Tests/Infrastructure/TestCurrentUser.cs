using Data.Database.Abstractions;

namespace Logic.Devices.Tests.Infrastructure;

/// <summary>
/// The organization the "request" works in – set by the test, read by the tenant filter. <c>null</c> = an anonymous
/// device request, like in production.
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public string Actor => SystemCurrentUser.SystemActor;

    public long? OrganizationId { get; set; }
}
