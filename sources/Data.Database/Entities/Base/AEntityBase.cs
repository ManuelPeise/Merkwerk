namespace Data.Database.Entities.Base;

/// <summary>
/// Base of every entity (ADR 011). Audit fields are set by <c>AuditSaveChangesInterceptor</c> – never by hand.
/// All times are UTC.
/// </summary>
public abstract class AEntityBase
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary><c>user:{id}</c>, <c>learner:{id}</c> or <c>system</c>.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    /// <summary><c>user:{id}</c>, <c>learner:{id}</c> or <c>system</c>.</summary>
    public string UpdatedBy { get; set; } = string.Empty;
}
