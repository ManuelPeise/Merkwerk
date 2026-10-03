namespace Data.Database.Abstractions;

/// <summary>
/// Who is acting right now. Used for audit fields and the organization query filter.
/// Implemented by Web.Core from the access token; <see cref="SystemCurrentUser"/> for jobs and tools.
/// </summary>
public interface ICurrentUser
{
    /// <summary><c>user:{id}</c>, <c>learner:{id}</c> or <c>system</c>.</summary>
    string Actor { get; }

    /// <summary>Organization of the caller; <c>null</c> = no organization, the query filter then returns no rows.</summary>
    long? OrganizationId { get; }
}
