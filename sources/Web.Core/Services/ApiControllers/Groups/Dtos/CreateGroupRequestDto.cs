using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Groups.Dtos;

/// <summary>learnerIds: children of the own family only (may be empty).</summary>
public sealed record CreateGroupRequestDto(
    [Required, MaxLength(50)] string Name,
    [Required, MaxLength(50)] IReadOnlyList<long> LearnerIds);
