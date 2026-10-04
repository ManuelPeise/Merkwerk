using Data.Database.Converters;
using Data.Database.Entities.Exercises;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class ExerciseVersionConfiguration : IEntityTypeConfiguration<ExerciseVersionEntity>
{
    public void Configure(EntityTypeBuilder<ExerciseVersionEntity> builder)
    {
        builder.ToTable("ExerciseVersions");
        builder.Property(v => v.Content).HasJsonColumn();
        builder.HasIndex(v => new { v.ExerciseId, v.Number }).IsUnique();
        builder.HasOne<ExerciseEntity>().WithMany().HasForeignKey(v => v.ExerciseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(v => v.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
