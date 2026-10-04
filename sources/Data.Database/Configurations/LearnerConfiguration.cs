using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class LearnerConfiguration : IEntityTypeConfiguration<LearnerEntity>
{
    public void Configure(EntityTypeBuilder<LearnerEntity> builder)
    {
        builder.ToTable("Learners");
        builder.Property(l => l.DisplayName).HasMaxLength(LearnerEntity.DisplayNameMaxLength).IsRequired();
        builder.Property(l => l.AvatarId).HasMaxLength(LearnerEntity.AvatarIdMaxLength).IsRequired();
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(l => l.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
