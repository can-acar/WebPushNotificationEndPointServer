namespace Lib;

using System;
using System.Security.Cryptography;
using Jose;

public static class VapidHelper
{
    public static string GetVapidAuthorizationHeader(string audience, string subject, string privateKey, string publicKey)
    {
        var extraHeaders = new Dictionary<string, object> {{"alg", "ES256"}};


        //alg = "ES256",
        // ecdh_public = publicKey

        var payload = new
        {
            aud = audience,
            exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
            sub = subject
        };

        return "vapid t=" + JWT.Encode(payload, EcdsaKey(privateKey), JwsAlgorithm.ES256, extraHeaders) + ", k=" + publicKey;
    }

    private static CngKey EcdsaKey(string privateKey)
    {
        return CngKey.Import(Convert.FromBase64String(privateKey), CngKeyBlobFormat.Pkcs8PrivateBlob);
    }
}