using System.ComponentModel.DataAnnotations;
using Data.Database.Entities.Devices;

namespace Web.Core.Services.ApiControllers.Devices.Dtos;

public sealed record PairDeviceRequestDto(
    [Required, RegularExpression("^[0-9]{6}$")] string Code,
    [MaxLength(Device.NameMaxLength)] string? DeviceName);
