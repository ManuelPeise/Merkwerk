using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Groups.Dtos;

/// <summary>learnerIds replaces the children of the group (may be empty).</summary>
public sealed record UpdateGroupRequestDto(
    [Range(1, long.MaxValue)] long Id,
    [Required, MaxLength(50)] string Name,
    [Required, MaxLength(50)] IReadOnlyList<long> LearnerIds);
