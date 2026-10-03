using Data.Database;
using Data.Database.Abstractions;
using Data.Database.Entities.Base;
using Data.Database.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Data.IntegrationTests.Infrastructure;

/// <summary>Organization-scoped entity that exists only in tests (real ones arrive with LP-105).</summary>
public sealed class TestNote : AOrganizationEntityBase
{
    public string Text { get; set; } = string.Empty;

    public DateTime? DueAt { get; set; }
}

/// <summary>MerkwerkDbContext plus <see cref="TestNote"/>; filters, conventions and audit come from the real context.</summary>
public sealed class TestDbContext : MerkwerkDbContext
{
    private TestDbContext(DbContextOptions<TestDbContext> options, ICurrentUser currentUser)
        : base(options, currentUser)
    {
    }

    public DbSet<TestNote> Notes => Set<TestNote>();

    public static TestDbContext Create(string connectionString, ICurrentUser currentUser, TimeProvider? timeProvider = null)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseMySQL(connectionString)
            .AddInterceptors(new AuditSaveChangesInterceptor(currentUser, timeProvider ?? TimeProvider.System))
            .Options;

        return new TestDbContext(options, currentUser);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register the test entity first, so the base class applies the organization filter to it as well.
        modelBuilder.Entity<TestNote>(entity =>
        {
            entity.ToTable("TestNotes");
            entity.Property(n => n.Text).HasMaxLength(200).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
