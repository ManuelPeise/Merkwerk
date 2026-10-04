using Data.Database.Entities.Groups;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<GroupEntity>
{
    public void Configure(EntityTypeBuilder<GroupEntity> builder)
    {
        builder.ToTable("Groups");
        builder.Property(g => g.Name).HasMaxLength(GroupEntity.NameMaxLength).IsRequired();
        builder.HasIndex(g => new { g.OrganizationId, g.Name }).IsUnique();
        builder.HasMany(g => g.Learners).WithOne().HasForeignKey(l => l.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(g => g.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
