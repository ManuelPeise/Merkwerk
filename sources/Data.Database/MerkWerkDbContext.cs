using System.Linq.Expressions;
using Data.Database.Abstractions;
using Data.Database.Converters;
using Data.Database.Entities.Base;
using Data.Database.Entities.Devices;
using Data.Database.Entities.Identity;
using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;
using Data.Database.Entities.Subjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Database;

/// <summary>
/// EF model of Merkwerk. Reached only through Data.Accessor (unit of work), never from Logic directly (ADR 012).
/// Exception: ASP.NET Core Identity's UserManager uses this context through its own store (LP-104).
/// </summary>
public class MerkwerkDbContext : IdentityUserContext<User, long>
{
    private const int ActorMaxLength = 64;

    /// <summary>MySQL limits index keys; Identity's string keys would otherwise become LONGTEXT.</summary>
    private const int IdentityKeyMaxLength = 128;

    private readonly ICurrentUser _currentUser;

    public MerkwerkDbContext(DbContextOptions<MerkwerkDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    /// <summary>For derived contexts (tests).</summary>
    protected MerkwerkDbContext(DbContextOptions options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Membership> Memberships => Set<Membership>();

    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DbSet<Learner> Learners => Set<Learner>();

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<PairingCode> PairingCodes => Set<PairingCode>();

    public DbSet<LearnerSession> LearnerSessions => Set<LearnerSession>();

    /// <summary>Read by the query filter on every query (EF parameterizes context members).</summary>
    protected long? CurrentOrganizationId => _currentUser.OrganizationId;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<UtcDateTimeOffsetConverter>();
        configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<NullableUtcDateTimeOffsetConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity first (users, claims, logins, tokens), then our tables and conventions.
        base.OnModelCreating(modelBuilder);
        ConfigureIdentityTables(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MerkwerkDbContext).Assembly);

        // Root entity types only: filters and base columns belong on the root of an inheritance hierarchy.
        var rootTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => t.BaseType is null && typeof(AEntityBase).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType)
            .ToList();

        foreach (var clrType in rootTypes)
        {
            var entity = modelBuilder.Entity(clrType);
            entity.Property(nameof(AEntityBase.CreatedBy)).HasMaxLength(ActorMaxLength).IsRequired();
            entity.Property(nameof(AEntityBase.UpdatedBy)).HasMaxLength(ActorMaxLength).IsRequired();

            if (typeof(AOrganizationEntityBase).IsAssignableFrom(clrType))
            {
                entity.HasIndex(nameof(AOrganizationEntityBase.OrganizationId));
                entity.HasQueryFilter(BuildOrganizationFilter(clrType));
            }
        }
    }

    /// <summary>Short table names instead of AspNet*, and bounded key lengths for MySQL.</summary>
    private static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<IdentityUserClaim<long>>().ToTable("UserClaims");

        modelBuilder.Entity<IdentityUserLogin<long>>(login =>
        {
            login.ToTable("UserLogins");
            login.Property(l => l.LoginProvider).HasMaxLength(IdentityKeyMaxLength);
            login.Property(l => l.ProviderKey).HasMaxLength(IdentityKeyMaxLength);
        });

        modelBuilder.Entity<IdentityUserToken<long>>(token =>
        {
            token.ToTable("UserTokens");
            token.Property(t => t.LoginProvider).HasMaxLength(IdentityKeyMaxLength);
            token.Property(t => t.Name).HasMaxLength(IdentityKeyMaxLength);
        });
    }

    /// <summary>e => e.OrganizationId == this.CurrentOrganizationId (null → no rows).</summary>
    private LambdaExpression BuildOrganizationFilter(Type clrType)
    {
        var entity = Expression.Parameter(clrType, "e");
        var organizationId = Expression.Convert(
            Expression.Property(entity, nameof(AOrganizationEntityBase.OrganizationId)),
            typeof(long?));
        var currentOrganizationId = Expression.Property(Expression.Constant(this), nameof(CurrentOrganizationId));

        return Expression.Lambda(Expression.Equal(organizationId, currentOrganizationId), entity);
    }
}
