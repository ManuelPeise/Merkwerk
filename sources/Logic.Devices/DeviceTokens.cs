using System.Security.Cryptography;
using System.Text;

namespace Logic.Devices;

/// <summary>Random device and session tokens, six-digit pairing codes and their hashes (only hashes are stored).</summary>
internal static class DeviceTokens
{
    public static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string CreatePairingCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);

    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
