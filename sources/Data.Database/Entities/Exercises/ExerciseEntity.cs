using Data.Database.Entities.Base;
using Shared.Enums;
using Shared.Models.Exercises.Generators;

namespace Data.Database.Entities.Exercises;

/// <summary>
/// The editable draft of an exercise (LP-110). Publishing freezes it as an <see cref="ExerciseVersionEntity"/>; the draft
/// stays editable and <see cref="HasUnpublishedChanges"/> tells whether it differs from the latest version.
/// </summary>
public sealed class ExerciseEntity : AOrganizationEntityBase
{
    public const int TitleMaxLength = 100;

    public string Title { get; set; } = string.Empty;

    /// <summary>Instance-wide subject (LP-109).</summary>
    public long SubjectId { get; set; }

    /// <summary>1–4.</summary>
    public int Grade { get; set; }

    public ExerciseContentSource ContentSource { get; set; }

    /// <summary>Number of the latest published version; 0 = never published.</summary>
    public int LatestVersion { get; set; }

    public bool HasUnpublishedChanges { get; set; }

    public bool IsArchived { get; set; }

    /// <summary>Settings for <see cref="ExerciseContentSource.Generator"/> (JSON column, LP-131); null otherwise.</summary>
    public GeneratorSettings? Generator { get; set; }

    /// <summary>The draft's questions; display order by <see cref="QuestionEntity.Position"/>.</summary>
    public List<QuestionEntity> Questions { get; set; } = [];
}
