using Data.Database.Entities.Organizations;

namespace Data.Accessor.Abstractions;

/// <summary>
/// Specialized repository: queries that Logic needs often get a named method here, so services can be unit-tested
/// with a mocked method instead of a mocked <c>IQueryable</c>.
/// </summary>
public interface IOrganizationRepository : IRepository<OrganizationEntity>
{
    /// <summary><c>true</c> once any family or school exists (setup is done, LP-105).</summary>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
