using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;

namespace Flashcards.Infrastructure;

internal sealed class PaginationTokenCodec
{
    private readonly byte[] _signingKeyBytes;

    public PaginationTokenCodec(string signingKey)
    {
        _signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);
    }

    public string Serialize(Dictionary<string, AttributeValue> lastEvaluatedKey)
    {
        var payload = JsonSerializer.Serialize(lastEvaluatedKey.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.S));
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var signature = ComputeSignature(payloadBytes);
        return $"{ToBase64Url(payloadBytes)}.{ToBase64Url(signature)}";
    }

    public Dictionary<string, AttributeValue> Deserialize(string token)
    {
        var parts = token.Split('.', 2);
        if (parts.Length != 2)
            throw new ArgumentException("Invalid pagination token.", nameof(token));

        var payloadBytes = FromBase64Url(parts[0]);
        var signatureBytes = FromBase64Url(parts[1]);
        var expectedSignature = ComputeSignature(payloadBytes);

        if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
            throw new ArgumentException("Invalid pagination token.", nameof(token));

        var payload = Encoding.UTF8.GetString(payloadBytes);
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(payload)
            ?? throw new ArgumentException("Invalid pagination token.", nameof(token));

        return dict.ToDictionary(kvp => kvp.Key, kvp => new AttributeValue { S = kvp.Value });
    }

    private byte[] ComputeSignature(byte[] payload)
    {
        using var hmac = new HMACSHA256(_signingKeyBytes);
        return hmac.ComputeHash(payload);
    }

    private static string ToBase64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static byte[] FromBase64Url(string encoded)
    {
        var base64 = encoded.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
