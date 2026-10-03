using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

public sealed record AdminResetPasswordRequestDto([Range(1, long.MaxValue)] long UserId);
