using Data.Database.Entities.Base;

namespace Data.Accessor.Abstractions;

/// <summary>
/// One business operation against the database (ADR 012): owns its own DbContext, which is disposed with the
/// unit of work. Usage: <c>await using var uow = _uowFactory.Create();</c>
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IOrganizationRepository Organizations { get; }

    IMembershipRepository Memberships { get; }

    IInvitationRepository Invitations { get; }

    ILearnerRepository Learners { get; }

    IDeviceRepository Devices { get; }

    IPairingCodeRepository PairingCodes { get; }

    ILearnerSessionRepository LearnerSessions { get; }

    /// <summary>Repository for <typeparamref name="T"/>; returns the specialized one if it exists (e.g. <see cref="Organizations"/>).</summary>
    IRepository<T> Repository<T>()
        where T : AEntityBase;

    /// <summary>Saves all changes of this unit of work in one transaction and returns the number of written rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
