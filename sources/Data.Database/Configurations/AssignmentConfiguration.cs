using Data.Database.Converters;
using Data.Database.Entities.Assignments;
using Data.Database.Entities.Exercises;
using Data.Database.Entities.Groups;
using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class AssignmentConfiguration : IEntityTypeConfiguration<AssignmentEntity>
{
    public void Configure(EntityTypeBuilder<AssignmentEntity> builder)
    {
        builder.ToTable("Assignments");
        builder.Property(a => a.Generator).HasOptionalJsonColumn();

        // One assignment per exercise and child or group; assigning again changes it (MySQL allows several NULLs).
        builder.HasIndex(a => new { a.ExerciseId, a.LearnerId }).IsUnique();
        builder.HasIndex(a => new { a.ExerciseId, a.GroupId }).IsUnique();

        // A deleted child or group takes its assignments along (LP-114).
        builder.HasOne<ExerciseEntity>().WithMany().HasForeignKey(a => a.ExerciseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<LearnerEntity>().WithMany().HasForeignKey(a => a.LearnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GroupEntity>().WithMany().HasForeignKey(a => a.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(a => a.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
