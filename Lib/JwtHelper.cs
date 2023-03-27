using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Lib;

public static class JwtHelper
{
    public static string GenerateJwt(string audience, string subject, string publicKey, string privateKey)
    {
        var header = new
        {
            alg = "ES256",
            typ = "JWT"
        };

        var expiry = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds();

        var payload = new
        {
            aud = audience,
            exp = expiry,
            sub = subject
        };

      
        var serializer = new JsonSerializer();

        var headerBytes = Encoding.UTF8.GetBytes(serializer.Serialize( header));
        var payloadBytes = Encoding.UTF8.GetBytes(serializer.Serialize( payload));

        var encodedHeader = Base64UrlEncode(headerBytes);
        var encodedPayload = Base64UrlEncode(payloadBytes);

        var dataToSign = $"{encodedHeader}.{encodedPayload}";
        var signature = Sign(Encoding.UTF8.GetBytes(dataToSign), privateKey);

        var encodedSignature = Base64UrlEncode(signature);

        return $"{encodedHeader}.{encodedPayload}.{encodedSignature}";
    }

    private static byte[] Sign(byte[] data, string privateKey)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
        return ecdsa.SignData(data, HashAlgorithmName.SHA256);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}


// public static class VapidHelper
// {
//     public static string GetVapidAuthorizationHeader(string audience, string subject, string privateKey, string publicKey)
//     {
//         var extraHeaders = new Dictionary<string, object> {{"alg", "ES256"}};
//
//
//         //alg = "ES256",
//         // ecdh_public = publicKey
//
//         var payload = new
//         {
//             aud = audience,
//             exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
//             sub = subject
//         };
//
//         return "vapid t=" + JWT.Encode(payload, EcdsaKey(privateKey), JwsAlgorithm.ES256, extraHeaders) + ", k=" + publicKey;
//     }
//
//     private static CngKey EcdsaKey(string privateKey)
//     {
//         return CngKey.Import(Convert.FromBase64String(privateKey), CngKeyBlobFormat.Pkcs8PrivateBlob);
//     }
// }