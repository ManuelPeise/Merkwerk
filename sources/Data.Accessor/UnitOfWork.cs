using Data.Accessor.Abstractions;
using Data.Accessor.Repositories;
using Data.Database;
using Data.Database.Entities.Base;
using Data.Database.Entities.Devices;
using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;

namespace Data.Accessor;

internal sealed class UnitOfWork : IUnitOfWork
{
    // One repository instance per entity type and unit of work.
    private readonly Dictionary<Type, object> _repositories = [];

    private readonly MerkwerkDbContext _context;

    public UnitOfWork(MerkwerkDbContext context)
    {
        _context = context;
    }

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
        _context.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => _context.DisposeAsync();

    /// <summary>Register every specialized repository here, so both access paths return the same instance.</summary>
    private object CreateRepository<T>()
        where T : AEntityBase
    {
        if (typeof(T) == typeof(OrganizationEntity))
        {
            return new OrganizationRepository(_context);
        }

        if (typeof(T) == typeof(MembershipEntity))
        {
            return new MembershipRepository(_context);
        }

        if (typeof(T) == typeof(InvitationEntity))
        {
            return new InvitationRepository(_context);
        }

        if (typeof(T) == typeof(LearnerEntity))
        {
            return new LearnerRepository(_context);
        }

        if (typeof(T) == typeof(DeviceEntity))
        {
            return new DeviceRepository(_context);
        }

        if (typeof(T) == typeof(PairingCodeEntity))
        {
            return new PairingCodeRepository(_context);
        }

        if (typeof(T) == typeof(LearnerSessionEntity))
        {
            return new LearnerSessionRepository(_context);
        }

        return new EntityRepository<T>(_context);
    }
}
