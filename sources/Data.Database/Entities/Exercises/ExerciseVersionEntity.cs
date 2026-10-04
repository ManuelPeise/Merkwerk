using Data.Database.Entities.Base;
using Shared.Models.Exercises;

namespace Data.Database.Entities.Exercises;

/// <summary>
/// A published, frozen version of an exercise (ADR 005, LP-110): the whole content as one JSON column. Never updated –
/// assignments (LP-114) and attempts point to it.
/// </summary>
public sealed class ExerciseVersionEntity : AOrganizationEntityBase
{
    public long ExerciseId { get; set; }

    /// <summary>1, 2, … per exercise.</summary>
    public int Number { get; set; }

    /// <summary>UTC.</summary>
    public DateTime PublishedAt { get; set; }

    public ExerciseSnapshot Content { get; set; } = null!;
}
