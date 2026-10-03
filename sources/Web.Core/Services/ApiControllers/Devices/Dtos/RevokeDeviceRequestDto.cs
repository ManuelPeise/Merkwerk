using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Devices.Dtos;

public sealed record RevokeDeviceRequestDto([Range(1, long.MaxValue)] long Id);
