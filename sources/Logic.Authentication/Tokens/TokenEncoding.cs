using System.Security.Cryptography;
using System.Text;

namespace Logic.Authentication.Tokens;

/// <summary>URL-safe transport of Identity tokens in mail links, and hashing of refresh tokens.</summary>
internal static class TokenEncoding
{
    public static string EncodeForUrl(string token) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(token)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>Returns <c>null</c> for anything that is not a token we encoded.</summary>
    public static string? DecodeFromUrl(string encoded)
    {
        var base64 = encoded.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
