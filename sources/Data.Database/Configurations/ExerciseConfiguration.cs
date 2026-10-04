using Data.Database.Entities.Exercises;
using Data.Database.Entities.Organizations;
using Data.Database.Entities.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class ExerciseConfiguration : IEntityTypeConfiguration<ExerciseEntity>
{
    public void Configure(EntityTypeBuilder<ExerciseEntity> builder)
    {
        builder.ToTable("Exercises");
        builder.Property(e => e.Title).HasMaxLength(ExerciseEntity.TitleMaxLength).IsRequired();
        builder.Property(e => e.ContentSource).HasConversion<string>().HasMaxLength(20);
        builder.HasMany(e => e.Questions).WithOne().HasForeignKey(q => q.ExerciseId).OnDelete(DeleteBehavior.Cascade);

        // Subjects in use cannot disappear under an exercise (deleting subjects comes with archiving rules later).
        builder.HasOne<SubjectEntity>().WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
