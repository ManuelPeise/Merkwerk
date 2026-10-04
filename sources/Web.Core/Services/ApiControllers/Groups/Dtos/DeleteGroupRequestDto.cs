using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Groups.Dtos;

public sealed record DeleteGroupRequestDto([Range(1, long.MaxValue)] long Id);
