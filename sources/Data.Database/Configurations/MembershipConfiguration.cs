using Data.Database.Entities.Identity;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(m => new { m.OrganizationId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);
        builder.HasOne<Organization>().WithMany().HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
