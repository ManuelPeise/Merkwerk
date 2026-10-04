using Data.Database.Converters;
using Data.Database.Entities.Exercises;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<QuestionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("Questions");
        builder.Property(q => q.Payload).HasJsonColumn();
        builder.Property(q => q.Solution).HasJsonColumn();
        builder.HasIndex(q => new { q.ExerciseId, q.Position });
        builder.HasOne<OrganizationEntity>().WithMany().HasForeignKey(q => q.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
