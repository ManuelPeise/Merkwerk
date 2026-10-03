using Data.Accessor.Abstractions;
using Data.Accessor.Repositories;
using Data.Database;
using Data.Database.Entities.Base;
using Data.Database.Entities.Devices;
using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;

namespace Data.Accessor;

internal sealed class UnitOfWork(MerkwerkDbContext context) : IUnitOfWork
{
    // One repository instance per entity type and unit of work.
    private readonly Dictionary<Type, object> _repositories = [];

    public IOrganizationRepository Organizations => (IOrganizationRepository)Repository<OrganizationEntity>();

    public IMembershipRepository Memberships => (IMembershipRepository)Repository<MembershipEntity>();

    public IInvitationRepository Invitations => (IInvitationRepository)Repository<InvitationEntity>();

    public ILearnerRepository Learners => (ILearnerRepository)Repository<LearnerEntity>();

    public IDeviceRepository Devices => (IDeviceRepository)Repository<DeviceEntity>();

    public IPairingCodeRepository PairingCodes => (IPairingCodeRepository)Repository<PairingCodeEntity>();

    public ILearnerSessionRepository LearnerSessions => (ILearnerSessionRepository)Repository<LearnerSessionEntity>();

    public IRepository<T> Repository<T>()
        where T : AEntityBase
    {
        if (!_repositories.TryGetValue(typeof(T), out var repository))
        {
            repository = CreateRepository<T>();
            _repositories.Add(typeof(T), repository);
        }

        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => context.DisposeAsync();

    /// <summary>Register every specialized repository here, so both access paths return the same instance.</summary>
    private object CreateRepository<T>()
        where T : AEntityBase
    {
        if (typeof(T) == typeof(OrganizationEntity))
        {
            return new OrganizationRepository(context);
        }

        if (typeof(T) == typeof(MembershipEntity))
        {
            return new MembershipRepository(context);
        }

        if (typeof(T) == typeof(InvitationEntity))
        {
            return new InvitationRepository(context);
        }

        if (typeof(T) == typeof(LearnerEntity))
        {
            return new LearnerRepository(context);
        }

        if (typeof(T) == typeof(DeviceEntity))
        {
            return new DeviceRepository(context);
        }

        if (typeof(T) == typeof(PairingCodeEntity))
        {
            return new PairingCodeRepository(context);
        }

        if (typeof(T) == typeof(LearnerSessionEntity))
        {
            return new LearnerSessionRepository(context);
        }

        return new EntityRepository<T>(context);
    }
}
