using Data.Database.Entities.Devices;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class LearnerSessionConfiguration : IEntityTypeConfiguration<LearnerSessionEntity>
{
    public void Configure(EntityTypeBuilder<LearnerSessionEntity> builder)
    {
        builder.ToTable("LearnerSessions");
        builder.Property(s => s.TokenHash).HasMaxLength(LearnerSessionEntity.TokenHashLength).IsFixedLength().IsRequired();
        builder.HasIndex(s => s.TokenHash).IsUnique();
        builder.HasOne(s => s.Device).WithMany().HasForeignKey(s => s.DeviceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.Learner).WithMany().HasForeignKey(s => s.LearnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
