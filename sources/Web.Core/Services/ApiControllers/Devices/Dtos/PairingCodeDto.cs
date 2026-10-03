using Logic.Devices.Pairing;

namespace Web.Core.Services.ApiControllers.Devices.Dtos;

/// <summary>Code for the tablet; <c>pairingUrl</c> goes into the QR code (generated in the client).</summary>
public sealed record PairingCodeDto(string Code, DateTimeOffset ExpiresAt, string PairingUrl)
{
    public static PairingCodeDto From(PairingCodeInfo info) => new(info.Code, info.ExpiresAt, info.PairingUrl);
}
