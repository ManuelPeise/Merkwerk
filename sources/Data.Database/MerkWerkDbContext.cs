using System.Linq.Expressions;
using Data.Database.Abstractions;
using Data.Database.Converters;
using Data.Database.Entities.Base;
using Data.Database.Entities.Organizations;
using Data.Database.Entities.Subjects;
using Microsoft.EntityFrameworkCore;

namespace Data.Database;

/// <summary>
/// EF model of Merkwerk. Reached only through Data.Accessor (unit of work), never from Logic directly (ADR 012).
/// </summary>
public class MerkwerkDbContext : DbContext
{
    private const int ActorMaxLength = 64;

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

    /// <summary>Read by the query filter on every query (EF parameterizes context members).</summary>
    protected long? CurrentOrganizationId => _currentUser.OrganizationId;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
