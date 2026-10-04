namespace Shared.Models.Devices;

/// <summary>A new pairing code for the parents' area; <see cref="PairingUrl"/> goes into the QR code.</summary>
public sealed record PairingCodeInfo(string Code, DateTimeOffset ExpiresAt, string PairingUrl);
