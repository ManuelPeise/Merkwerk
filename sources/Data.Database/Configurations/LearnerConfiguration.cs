using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class LearnerConfiguration : IEntityTypeConfiguration<Learner>
{
    public void Configure(EntityTypeBuilder<Learner> builder)
    {
        builder.ToTable("Learners");
        builder.Property(l => l.DisplayName).HasMaxLength(Learner.DisplayNameMaxLength).IsRequired();
        builder.Property(l => l.AvatarId).HasMaxLength(Learner.AvatarIdMaxLength).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(l => l.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
