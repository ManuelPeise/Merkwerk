using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Assignments.Dtos;

public sealed record RevokeAssignmentRequestDto([Range(1, long.MaxValue)] long Id);
