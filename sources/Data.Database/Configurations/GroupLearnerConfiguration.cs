using Data.Database.Entities.Groups;
using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class GroupLearnerConfiguration : IEntityTypeConfiguration<GroupLearnerEntity>
{
    public void Configure(EntityTypeBuilder<GroupLearnerEntity> builder)
    {
        builder.ToTable("GroupLearners");
        builder.HasIndex(l => new { l.GroupId, l.LearnerId }).IsUnique();

        // A deleted child disappears from every group (LP-108).
        builder.HasOne<LearnerEntity>().WithMany().HasForeignKey(l => l.LearnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(l => l.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
