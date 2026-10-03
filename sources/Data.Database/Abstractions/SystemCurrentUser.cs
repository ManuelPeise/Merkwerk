namespace Data.Database.Abstractions;

/// <summary>Actor for background jobs, seeding and tools. Sees no organization-scoped rows.</summary>
public sealed class SystemCurrentUser : ICurrentUser
{
    public const string SystemActor = "system";

    public static SystemCurrentUser Instance { get; } = new();

    public string Actor => SystemActor;

    public long? OrganizationId => null;
}
