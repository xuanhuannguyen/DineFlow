using System.Security.Cryptography;
using System.Text;
using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.Api.Security;

public sealed class ApiTokenService : IApiTokenService
{
    private readonly IConfiguration _configuration;

    public ApiTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(CurrentUserDto user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds();
        var payload = $"{user.UserId}|{user.Username}|{user.Role}|{expiresAt}";
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signature = Sign(encodedPayload);

        return $"{encodedPayload}.{signature}";
    }

    public bool TryReadRole(string token, out UserRole role)
    {
        role = default;
        try
        {
            var parts = token.Split('.', 2);
            if (parts.Length != 2 || !FixedTimeEquals(parts[1], Sign(parts[0])))
            {
                return false;
            }

            var payload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
            var values = payload.Split('|');
            if (values.Length != 4
                || !Enum.TryParse(values[2], ignoreCase: true, out role)
                || !long.TryParse(values[3], out var expiresAt))
            {
                return false;
            }

            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= expiresAt;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private string Sign(string encodedPayload)
    {
        var secret = _configuration["Security:ApiTokenSecret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Security:ApiTokenSecret is not configured.");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(encodedPayload)));
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}
