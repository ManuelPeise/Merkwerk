using Data.Database.Abstractions;
using Data.Database.Entities.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Database.Configurations;

internal sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    /// <summary>Fixed values: seed data must not change between two migrations (LP-103).</summary>
    private static readonly DateTime SeededAt = new(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");
        builder.Property(s => s.Name).HasMaxLength(Subject.NameMaxLength).IsRequired();
        builder.Property(s => s.LanguageCode).HasMaxLength(Subject.LanguageCodeMaxLength).IsRequired();
        builder.Property(s => s.Color).HasMaxLength(Subject.ColorMaxLength).IsRequired();
        builder.Property(s => s.Icon).HasMaxLength(Subject.IconMaxLength).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();

        // Standard subjects (LP-103). Colors are the subject tokens of LP-008.
        builder.HasData(
            Seed(1, "Deutsch", "de", "subject.german", "german"),
            Seed(2, "Englisch", "en", "subject.english", "english"),
            Seed(3, "Mathe", "de", "subject.math", "math"));
    }

    private static Subject Seed(long id, string name, string languageCode, string color, string icon) => new()
    {
        Id = id,
        Name = name,
        LanguageCode = languageCode,
        Color = color,
        Icon = icon,
        CreatedAt = SeededAt,
        CreatedBy = SystemCurrentUser.SystemActor,
        UpdatedAt = SeededAt,
        UpdatedBy = SystemCurrentUser.SystemActor,
    };
}
