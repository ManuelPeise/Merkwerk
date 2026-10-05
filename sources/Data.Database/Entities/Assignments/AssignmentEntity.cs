using Data.Database.Entities.Base;
using Shared.Models.Exercises.Generators;

namespace Data.Database.Entities.Assignments;

/// <summary>
/// An exercise assigned to one child or to one group (LP-114) – exactly one of <see cref="LearnerId"/> and
/// <see cref="GroupId"/> is set (checked by the service; MySQL forbids CHECK constraints on cascading foreign keys). A
/// group assignment is dynamic: it reaches whoever is in the group at the time. Children always get the latest published
/// version of the exercise; the attempt remembers which one (LP-115).
/// </summary>
public sealed class AssignmentEntity : AOrganizationEntityBase
{
    public long ExerciseId { get; set; }

    public long? LearnerId { get; set; }

    public long? GroupId { get; set; }

    /// <summary>Optional, a calendar day without time.</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>The child may open the dot array (LP-133) while practising.</summary>
    public bool AllowDotArray { get; set; }

    /// <summary>The child may open the times table matrix (LP-134) while practising.</summary>
    public bool AllowTimesTableMatrix { get; set; }

    /// <summary>
    /// Settings that replace the exercise's own generator settings for this assignment (JSON column); only for
    /// generator exercises, null = use the exercise's settings.
    /// </summary>
    public GeneratorSettings? Generator { get; set; }
}
